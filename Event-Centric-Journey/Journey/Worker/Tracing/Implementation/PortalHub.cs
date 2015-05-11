using Journey.Worker.Portal;
using Microsoft.AspNet.SignalR;

namespace Journey.Worker.Tracing
{
    public class PortalHub : Hub
    {
        private readonly ITracer tracer;

        public PortalHub()
        {
            this.tracer = WorkerRoleWebPortal.Instance.WorkerRole.Tracer;
        }

        public void SendMessage(string message)
        {
            Clients.All.newMessage(message);

            if (tracer != null)
                this.tracer.TraceAsync(message);
        }
    }
}
