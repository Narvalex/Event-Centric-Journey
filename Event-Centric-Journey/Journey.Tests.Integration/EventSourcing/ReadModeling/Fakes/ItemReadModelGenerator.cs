using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Journey.Tests.Integration.EventSourcing.ReadModeling
{
    public class ItemReadModelGenerator : 
        IEventHandler<ItemAdded>
    {
        private readonly IReadModelGeneratorEngine<ItemReadModelDbContext> generator;

        public ItemReadModelGenerator(IReadModelGeneratorEngine<ItemReadModelDbContext> generator)
        {
            this.generator = generator;
        }

        public void Handle(ItemAdded e)
        {
            this.generator.Project(e,
            context => 
            {
                // It shoud not hit this.
                Assert.IsTrue(false);
            },
            context => 
            {
                context.Items.Add(new Item { UnidentifiableId = e.Id, Name = e.Name });
            });
        }
    }
}
