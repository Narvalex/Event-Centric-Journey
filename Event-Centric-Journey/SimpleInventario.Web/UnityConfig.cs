using Journey.Client;
using Journey.Database;
using Journey.EventSourcing.ReadModeling;
using Journey.Messaging;
using Journey.Serialization;
using Microsoft.Practices.Unity;
using SimpleInventario.Application;
using System;
using System.Data.Entity;

namespace SimpleInventario.Web.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            var serializer = new JsonTextSerializer();
            var config = DefaultClientApplicationConfigProvider.Configuration;

            container.RegisterInstance<ITextSerializer>(serializer);
            container.RegisterInstance<IClientApplicationConfig>(config);

            container.RegisterType<IMessageSender, MessageSender>(
                "CommandBus",
                new TransientLifetimeManager(),
                new InjectionConstructor(
                    Database.DefaultConnectionFactory,
                    config.BusConnectionString,
                    config.CommandBusTableName));

            container.RegisterType<ICommandBus, CommandBus>(
                new ContainerControlledLifetimeManager(), 
                new InjectionConstructor(
                    new ResolvedParameter<IMessageSender>("CommandBus"), serializer));

           
            Func<ReadModelDbContext> readModelContextFactory = () => new ReadModelDbContext(config.ReadModelConnectionString);

            container.RegisterType<IClientApplication, ClientApplication>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    container.Resolve<ICommandBus>(),
                    config.WorkerRoleStatusUrl,
                    readModelContextFactory,
                    config.EventualConsistencyCheckRetryPolicy));

            container.RegisterType<IInventarioApp, InventarioApp>();
        }
    }
}
