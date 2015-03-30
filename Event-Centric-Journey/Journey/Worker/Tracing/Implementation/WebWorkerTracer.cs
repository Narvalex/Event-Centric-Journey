using Journey.Worker.Tracing;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Journey.Worker
{
    public class WebWorkerTracer : IWebTracer
    {
        private static readonly Queue<Notification> Notifications = new Queue<Notification>(50);
        private static int NotificationCountLimit = 50;
        private static volatile int NotificationCount = default(int);
        private static object StreamingLockObject = new object();
        private static object IsRunningLockObject = new object();
        private static volatile bool IsRunning = false;

        public readonly Action<string> notify;

        public WebWorkerTracer()
        {
            this.notify = (n) =>
            {
                lock (StreamingLockObject)
                {
                    if (Notifications.Count >= NotificationCountLimit)
                        Notifications.Dequeue();

                    Notifications.Enqueue(new Notification
                    {
                        id = ++NotificationCount,
                        message = string.Format("{0} - {1}", DateTime.Now.ToString(), n)
                    });
                }
            };
        }

        public void Notify(string info)
        {
            Task.Factory.StartNew(() => this.notify(info), TaskCreationOptions.PreferFairness);
        }

        public void StartIfNotRunningNotificationStreaming(IHubCallerConnectionContext<dynamic> clients)
        {
            if (IsRunning)
                return;

            lock (IsRunningLockObject)
            {
                if (IsRunning)
                    return;

                IsRunning = true;
                var task = Task.Factory.StartNew(() => StartNotificationStreaming(clients), TaskCreationOptions.LongRunning);
            }
        }

        private void StartNotificationStreaming(IHubCallerConnectionContext<dynamic> clients)
        {
            while(CheckIfIsRunning())
            {
                try
                {
                    lock (StreamingLockObject)
                    {
                        if (Notifications.Any())
                        {
                            foreach (var notification in Notifications)
                            {
                                clients.All.notify(notification);                                                                
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // we catch all exceptions in order to continue to provide information
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        private static bool CheckIfIsRunning()
        {
            lock (IsRunningLockObject)
            {
                return IsRunning;
            }
        }

        public class Notification
        {
            public int id { get; set; }
            public string message { get; set; }
        }




        public void StopNotificationStreaming(IHubCallerConnectionContext<dynamic> clients)
        {
            lock (IsRunningLockObject)
            {
                IsRunning = false;
            }

            clients.All.newMessage("Client disconnected");

            var count = 0;
            while (count < 3)
            {
                ++count;
                Thread.Sleep(TimeSpan.FromSeconds(2));
                clients.All.newMessage("Scanning clients...");
            }
        }
    }
}
