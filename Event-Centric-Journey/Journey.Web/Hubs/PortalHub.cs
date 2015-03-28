using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Threading;
using Journey.Worker;

namespace Journey.Web.Hubs
{
    public class PortalHub : Hub
    {
        private static readonly object LockObject = new object();

        private static volatile bool IsRunning = false;

        private readonly IWorkerRoleTracer worker;

        public PortalHub()
        {
            this.worker = WorkerRoleManager.Instance.WorkerRole.Tracer;
        }

        public void SendMessage(string message)
        {
            Clients.All.newMessage(message);
        }

        public void Ping(string message)
        {
            StartIfIsNotYetRunning();
        }

        private void StartIfIsNotYetRunning()
        {
            if (IsRunning)
                return;

            lock (LockObject)
            {
                if (IsRunning)
                    return;

                IsRunning = true;
                var task = Task.Factory.StartNew(() => StartMessageSender(), TaskCreationOptions.LongRunning);
            }
        }

        /// <summary>
        /// I should read this:
        /// https://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim.aspx
        /// </summary>
        private void StartMessageSender()
        {
            while (CheckIfIsRunning())
            {
                try
                {
                    lock (WorkerRoleWebHost.LockObject)
                    {

                        if (WorkerRoleWebHost.Notifications.Any())
                        {
                            foreach (var notification in WorkerRoleWebHost.Notifications)
                                Clients.All.notify(notification);
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
            lock (LockObject)
            {
                return IsRunning;
            }
        }

        public override Task OnConnected()
        {
            this.StartIfIsNotYetRunning();
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            this.StartIfIsNotYetRunning();
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            lock (LockObject)
            {
                IsRunning = false;
            }

            Clients.All.newMessage("Client disconnected");

            var count = 0;
            while (count < 3)
            {
                ++count;
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Clients.All.newMessage("Scanning clients...");
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}