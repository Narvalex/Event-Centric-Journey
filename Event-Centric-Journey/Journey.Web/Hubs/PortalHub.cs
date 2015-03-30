using Journey.Worker;
using Journey.Worker.Tracing;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Journey.Web.Hubs
{
    public class PortalHub : Hub
    {
        private static IWebTracer tracer;

        public void SendMessage(string message)
        {
            Clients.All.newMessage(message);
        }

        public PortalHub()
        {
            if (tracer == null)
            tracer = WorkerRoleWebPortal.Instance.WorkerRole.Tracer as IWebTracer;
        }

        private void StartIfIsNotYetRunning()
        {
            tracer.StartIfNotRunningNotificationStreaming(this.Clients);
        }

        public void Ping(string message)
        {
            StartIfIsNotYetRunning();
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
            tracer.StopNotificationStreaming(this.Clients);
            return base.OnDisconnected(stopCalled);
        }
    }
}