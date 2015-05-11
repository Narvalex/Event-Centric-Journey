using Journey.Utils.SystemTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Journey.Worker.Tracing
{
    public class WebWorkerRoleTracer : SignalRBase<PortalHub>, IWorkerRoleTracer
    {
        private static readonly Queue<Notification> Notifications = new Queue<Notification>(50);
        private static int NotificationCountLimit = 50;
        private static volatile int NotificationCount = default(int);
        private readonly ISystemTime time;

        private static object lockObject = new object();

        public WebWorkerRoleTracer(ISystemTime time)
        {
            this.time = time;
        }

        public void Trace(string info)
        {
            ThreadPool.QueueUserWorkItem(x => this.Write(info));
        }

        private void Write(string info)
        {
            lock (lockObject)
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
        }

        public void Notify(string info)
        {
            ThreadPool.QueueUserWorkItem(x => this.Write(info));
        }


        public void Notify(IEnumerable<string> notifications)
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                foreach (var notification in notifications)
                    this.Write(notification);
            });
        }
    }
}
