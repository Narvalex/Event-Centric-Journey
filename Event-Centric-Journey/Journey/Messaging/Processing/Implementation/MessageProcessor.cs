using Journey.Serialization;
using Journey.Worker;
using System;
using System.IO;

namespace Journey.Messaging.Processing
{
    /// <summary>
    /// Provides basic common processing code for components that handle 
    /// incomming messages from a receiver;
    /// </summary>
    public abstract class MessageProcessor : IMessageProcessor, IDisposable
    {
        protected readonly IWorkerRoleTracer tracer;
        private readonly IMessageReceiver receiver;
        private readonly ITextSerializer serializer;
        private readonly IMessagingSettings settings;
        private readonly object lockObject = new object();
        private bool disposed;
        private bool started = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProcessor"/> class.
        /// </summary>
        public MessageProcessor(IMessageReceiver receiver, ITextSerializer serializer, IWorkerRoleTracer tracer)
        {
            this.receiver = receiver;
            this.serializer = serializer;
            this.tracer = tracer;
        }

        /// <summary>
        /// Starts the listener.
        /// </summary>
        public void Start()
        {
            this.ThrowIfDisposed();
            lock (this.lockObject)
            {
                if (!this.started)
                {
                    this.receiver.MessageReceived += OnMessageReceived;
                    this.receiver.Start();
                    this.started = true;
                }
            }
        }

        /// <summary>
        /// Stops the listener.
        /// </summary>
        public void Stop()
        {
            lock (this.lockObject)
            {
                if (this.started)
                {
                    this.receiver.Stop();
                    this.receiver.MessageReceived -= OnMessageReceived;
                    this.started = false;
                }
            }
        }

        /// <summary>
        /// Disposes the resources used by the processor.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Stop();
                    this.disposed = true;

                    using (this.receiver as IDisposable)
                    {
                        // Dispose receiver if it's disposable.
                    }
                }
            }
        }

        ~MessageProcessor()
        {
            this.Dispose(false);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {

            try
            {
                var body = this.Deserialize(args.Message.Body);

                this.tracer.Notify(string.Format("Message received!"));

                this.ProcessMessage(body, args.Message.CorrelationId);
            }
            catch (Exception e)
            {
                this.tracer.Notify(string.Format("An exception happened while processing message through handler/s:\r\n{0}", e));
                this.tracer.Notify("The message will be flagged as dead letter in the bus.");
                this.tracer.Notify("Error will be ignored and message receiving will continue.");

                throw;
            }
        }

        protected abstract void ProcessMessage(object payload, string correlationId);

        //protected string Serialize(object payload)
        //{
        //    using (var writer = new StringWriter())
        //    {
        //        this.serializer.Serialize(writer, payload);
        //        return writer.ToString();
        //    }
        //}

        protected object Deserialize(string serializedPayload)
        {
            using (var reader = new StringReader(serializedPayload))
            {
                return this.serializer.Deserialize(reader);
            }
        }

        //[Conditional("TRACE")]
        //private void TracePayload(object payload)
        //{
        //    Trace.WriteLine(this.Serialize(payload));
        //}

        private void ThrowIfDisposed()
        {
            if (this.disposed)
                throw new ObjectDisposedException("MessageProcessor");
        }
    }
}
