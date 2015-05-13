using Journey.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Journey.Messaging.Processing
{
    public class CommandingConcurrencyResolver
    {
        public static volatile bool ThrottlingDetected;
        private static volatile List<string> throttledHandlers = new List<string>();

        private static readonly object lockObject = new object();

        public void HandleConcurrentMessage(object payload, ICommandHandler handler)
        {
            ThrottlingDetected = true;
            var handlerName = handler.GetType().FullName;
            lock (lockObject)
            {
                if (!throttledHandlers.Where(h => h == handlerName).Any())
                    throttledHandlers.Add(handlerName);

                this.Handle(payload, handler);

                throttledHandlers.Remove(handlerName);
                ThrottlingDetected = false;
            }
        }

        public bool HandlerIsThrottled(ICommandHandler handler)
        {
            return throttledHandlers.Where(h => h == handler.GetType().FullName).Any();
        }

        private void Handle(object payload, ICommandHandler handler)
        {
            var attempts = default(int);
            var threshold = 10;
            while (true)
            {
                try
                {
                    ((dynamic)handler).Handle((dynamic)payload);
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
