using Journey.Worker;
using Journey.Messaging;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Moq;
using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Journey.Tests.Messaging.EventProcessorFixture
{
    public class GIVEN_event_processor
    {
        private Mock<IMessageReceiver> receiverMock = new Mock<IMessageReceiver>();
        private EventProcessor processor;

        public GIVEN_event_processor()
        {
            Trace.Listeners.Clear();
            this.processor = new EventProcessor(this.receiverMock.Object, CreateSerializer(), new ConsoleWorkerTracer());
        }


        [Fact]
        public void WHEN_starting_THEN_starts_receiver()
        {
            this.processor.Start();

            this.receiverMock.Verify(r => r.Start());
        }

        [Fact]
        public void WHEN_stopping_after_starting_THEN_stops_receiver()
        {
            this.processor.Start();
            this.processor.Stop();

            this.receiverMock.Verify(r => r.Stop());
        }

        [Fact]
        public void WHEN_receives_message_THEN_notifies_registered_handler()
        {
            var handlerAMock = new Mock<IEventHandler>();
            handlerAMock.As<IEventHandler<Event1>>();
            handlerAMock.As<IEventHandler<Event2>>();

            var handlerBMock = new Mock<IEventHandler>();
            handlerBMock.As<IEventHandler<Event2>>();

            this.processor.Register(handlerAMock.Object);
            this.processor.Register(handlerBMock.Object);

            this.processor.Start();

            var event1 = new Event1 { SourceId = Guid.NewGuid() };
            var event2 = new Event2 { SourceId = Guid.NewGuid() };

            this.receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event1))));
            this.receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event2))));

            handlerAMock.As<IEventHandler<Event1>>().Verify(h => h.Handle(It.Is<Event1>(e => e.SourceId == event1.SourceId)));
            handlerAMock.As<IEventHandler<Event2>>().Verify(h => h.Handle(It.Is<Event2>(e => e.SourceId == event2.SourceId)));
            handlerBMock.As<IEventHandler<Event2>>().Verify(h => h.Handle(It.Is<Event2>(e => e.SourceId == event2.SourceId)));
        }

        [Fact]
        public void WHEN_receives_message_THEN_notifies_generic_handler()
        {
            var handler = new Mock<IEventHandler>();
            handler.As<IEventHandler<IEvent>>();

            this.processor.Register(handler.Object);

            this.processor.Start();

            var event1 = new Event1 { SourceId = Guid.NewGuid() };
            var event2 = new Event2 { SourceId = Guid.NewGuid() };

            this.receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event1))));
            this.receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event2))));

            handler.As<IEventHandler<IEvent>>().Verify(h => h.Handle(It.Is<Event1>(e => e.SourceId == event1.SourceId)));
            handler.As<IEventHandler<IEvent>>().Verify(h => h.Handle(It.Is<Event2>(e => e.SourceId == event2.SourceId)));
        }

        [Fact]
        public void WHEN_throws_THEN_retries()
        {
            var handler = new TestableBuggyEventHandler();
            this.processor.Register(handler);

            // Registering also a generic handler
            var genericHandler = new Mock<IEventHandler>();
            genericHandler.As<IEventHandler<IEvent>>();

            this.processor.Register(genericHandler.Object);

            this.processor.Start();

            var event1 = new Event1 { SourceId = Guid.NewGuid() };
            var event2 = new Event2 { SourceId = Guid.NewGuid() };

            this.receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event1))));
            Assert.Throws(new AggregateException().GetType(), () => this.receiverMock.Raise(r => r.MessageReceived += null, new MessageReceivedEventArgs(new Message(Serialize(event2)))));
        }

        private string Serialize(object payload)
        {
            var serializer = CreateSerializer();

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, payload);
                return writer.ToString();
            }
        }

        private static ITextSerializer CreateSerializer()
        {
            return new JsonTextSerializer();
        }

        public class Event1 : IEvent
        {
            public Guid SourceId { get; set; }

            public DateTime SourceTimeStamp { get; set; }

            public DateTime TimeStamp { get; set; }
        }

        public class Event2 : IEvent
        {
            public Guid SourceId { get; set; }

            public DateTime SourceTimeStamp { get; set; }

            public DateTime TimeStamp { get; set; }
        }

        protected class TestableBuggyEventHandler : 
            IEventHandler<Event1>,
            IEventHandler<Event2>
        {

            public void Handle(Event1 e)
            {
                Console.WriteLine("Handling Event1....");
            }

            public void Handle(Event2 e)
            {
                Console.WriteLine("An error will ocurr on handling Event2....");
                throw new Exception();
            }
        }
    }
}
