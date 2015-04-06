using Journey.Tests.Integration.ReadModeling.ReadModelGeneratorFixture;
using Journey.Messaging.Processing;
using System;

namespace Journey.Tests.Integration.ReadModeling.Implementation
{
    public class ItemReadModelGenerator : IEventHandler<ItemAdded>
    {
        private readonly Func<ItemsDbContext> contextFactory;

        public ItemReadModelGenerator(Func<ItemsDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public void Handle(ItemAdded @event)
        {
            using (var repository = this.contextFactory.Invoke())
            {
                var dto = repository.Find<ItemView>(@event.ItemId);
                if (dto != null)
                {
                    // Ignore
                }
                else
                {
                    repository.Set<ItemView>().Add(
                        new ItemView
                        {
                            ItemId = @event.ItemId,
                            Name = @event.Name
                        });

                    repository.SaveChanges();
                }
            }
        }
    }
}
