
using Microsoft.AspNet.SignalR.Hubs;
namespace Journey.Worker.Tracing
{
    public interface IWebTracer : IWorkerRoleTracer
    {
        /// <summary>
        /// Envía todas las notificaciones a todos los clientes conectados al portal.
        /// </summary>
        void StartIfNotRunningNotificationStreaming(IHubCallerConnectionContext<dynamic> clients);

        void StopNotificationStreaming(IHubCallerConnectionContext<dynamic> clients);
    }
}
