using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Journey.Worker.Tracing
{
    public class WebWorkerRoleTracer : SignalRBase<PortalHub>, IWorkerRoleTracer
    {
        private static readonly Queue<Notification> Notifications = new Queue<Notification>(50);
        private static int NotificationCountLimit = 50;
        private static volatile int NotificationCount = default(int);

        private static object lockObject = new object();

        public void Notify(string info)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    lock (lockObject)
                    {
                        // Adding New Notification
                        if (Notifications.Count >= NotificationCountLimit)
                            Notifications.Dequeue();

                        Notifications.Enqueue(new Notification
                        {
                            id = ++NotificationCount,
                            message = string.Format("{0} - {1}", DateTime.Now.ToString(), info)
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
                catch (Exception)
                {
                    // we catch all exceptions in order to continue to provide information
                }
            }, TaskCreationOptions.PreferFairness);

        }

        public class Notification
        {
            public int id { get; set; }
            public string message { get; set; }
        }
    }
}
