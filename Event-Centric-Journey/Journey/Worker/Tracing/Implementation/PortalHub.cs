using Microsoft.AspNet.SignalR;

namespace Journey.Worker.Tracing
{
    public class PortalHub : Hub
    {
        private readonly IWorkerRolePortal portal;

        public PortalHub()
        {
            this.portal = WorkerRoleWebPortal.Instance;
        }

        public void SendMessage(string message)
        {
            Clients.All.newMessage(message);

            if (this.portal != null)
                this.portal.WorkerRole.Tracer.Notify(message);
        }
    }
}
