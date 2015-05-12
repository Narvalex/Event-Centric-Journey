using Journey.Utils.SystemTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Journey.Worker.Tracing
{
    public class WebTracer : SignalRBase<PortalHub>, ITracer
    {
        private const int NotificationCountLimit = 50;
        private static Queue<Notification> Notifications = new Queue<Notification>(NotificationCountLimit);
        private static volatile int NotificationCount = default(int);
        private readonly ISystemTime time;

        private Action<string> traceAsyncAction;
        private readonly Action<string> traceAsyncEnabled;
        private readonly Action<string> traceAsyncDisabled;

        private static object lockObject = new object();

        public WebTracer(ISystemTime time)
        {
            this.time = time;
            this.traceAsyncEnabled = (info) => ThreadPool.QueueUserWorkItem(x =>
                                    {
                                        lock (lockObject)
                                        {
                                            this.Write(info);
                                        }
                                    });

            this.traceAsyncDisabled = (info) => { };

            this.EnableTracing();
        }

        public void TraceAsync(string info)
        {
            this.traceAsyncAction.Invoke(info);
        }

        public void Notify(string info)
        {
            lock (lockObject)
            {
                this.Write(info);
            }
        }


        public void Notify(IEnumerable<string> notifications)
        {
            lock (lockObject)
            {
                foreach (var notification in notifications)
                    this.Write(notification);
            }
        }

        private void Write(string info)
        {
            // Adding New Notification
            if (Notifications.Count >= NotificationCountLimit)
                Notifications.Dequeue();

            Notifications.Enqueue(new Notification
            {
                id = ++NotificationCount,
                message = string.Format("{0} {1}", this.time.Now.ToString(), info)
            });

            // Publishing Notification
            if (Notifications.Any())
            {
                foreach (var notification in Notifications)
                {
                    this.Hub.Clients.All.notify(notification);
                }
            }
        }

        public void DisableTracing()
        {
            lock (lockObject)
            {
                this.DoDisableTracing();
            }
        }

        private void DoDisableTracing()
        {
            Notifications = new Queue<Notification>(NotificationCountLimit);
            this.traceAsyncAction = this.traceAsyncDisabled;
        }

        public void EnableTracing()
        {
            this.traceAsyncAction = this.traceAsyncEnabled;
        }
    }
}
