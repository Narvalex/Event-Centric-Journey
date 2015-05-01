using Journey.Worker.Portal;
using Microsoft.Practices.Unity;
using SimpleInventario.DomainRegistry;

namespace Journey.Web.App_Start
{
    partial class SystemContainer
    {
        static partial void RegisterWebPortal(IUnityContainer container)
        {
            var portal = SimpleInventarioWorkerRolePortalFactory.CreatePortal();
            container.RegisterInstance<IWorkerRoleWebPortal>(portal);
        }
    }
}


