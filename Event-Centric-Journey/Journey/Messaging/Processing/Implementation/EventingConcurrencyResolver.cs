using Journey.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Journey.Messaging.Processing
{
    public class EventingConcurrencyResolver
    {
        public static volatile bool ThrottlingDetected;
        private static volatile List<string> throttledHandlers = new List<string>();

        private static readonly object lockObject = new object();

        public void HandleConcurrentMessage<T>(Tuple<Type, Action<Envelope>> handler, Envelope<T> envelope)
        {
            ThrottlingDetected = true;
            var handlerName = this.GetHandlerName(handler);

            lock (lockObject)
            {
                if (!throttledHandlers.Where(h => h == handlerName).Any())
                    throttledHandlers.Add(handlerName);

                this.Handle(handler, envelope);

                throttledHandlers.Remove(handlerName);
                ThrottlingDetected = false;
            }
        }

        private string GetHandlerName(Tuple<Type, Action<Envelope>> handler)
        {
            return handler.Item1.FullName + "_" + handler.Item2.GetType().FullName;
        }

        public bool HandlerIsThrottled(Tuple<Type, Action<Envelope>> handler)
        {
            return throttledHandlers.Where(h => h == this.GetHandlerName(handler)).Any();
        }

        private void Handle<T>(Tuple<Type, Action<Envelope>> handler, Envelope<T> envelope)
        {
            var attempts = default(int);
            var threshold = 10;
            while (true)
            {
                try
                {
                    handler.Item2(envelope);
                    break;
                }
                catch (EventStoreConcurrencyException)
                {
                    ++attempts;
                    if (attempts > threshold)
                        throw;

                    Thread.Sleep(50 * attempts);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
