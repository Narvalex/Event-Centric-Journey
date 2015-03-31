using Microsoft.AspNet.SignalR;

namespace Journey.Worker.Tracing
{
    public class PortalHub : Hub
    {
        public void SendMessage(string message)
        {
            Clients.All.newMessage(message);

            if (WorkerRoleWebPortal.Instance != null)
                WorkerRoleWebPortal.Instance.WorkerRole.Tracer.Notify(message);
        }
    }
}
