using Infrastructure.CQRS.Database;
using System.Data.Entity;

namespace Infrastructure.CQRS.Messaging.Processing.Implementation
{
    public class EventSubscriptionDbContext : DbContext
    {
        static EventSubscriptionDbContext()
        {
            System.Data.Entity.Database.SetInitializer<EventSubscriptionDbContext>(null);
        }

        public EventSubscriptionDbContext(IConnectionStringProvider connectionProvider)
            : base(connectionProvider.ConnectionString)
        { }

        public EventSubscriptionDbContext()
            : base("Name=defaultConnection")
        { }
    }

    public class EventSubscription
    {
        public int MyProperty { get; set; }
    }
}
