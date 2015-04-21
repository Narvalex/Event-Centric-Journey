using Journey.EventSourcing;
using System;
using System.Collections.Generic;

namespace Journey.Tests.Integration.EventSourcing.ReadModeling
{
    public class ItemActor : EventSourced,
        IRehydratesFrom<ItemAdded>
    {
        public ItemActor(Guid id)
            : base(id)
        { }

        public ItemActor(Guid id, IEnumerable<IVersionedEvent> history)
            : base(id)
        {
            this.LoadFrom(history);
        }

        public void HandleCommands()
        {
            base.Update(new ItemAdded
            {
                Id = 1,
                Name = "Chair",
            });

            base.Update(new ItemAdded
            {
                Id = 2,
                Name = "Table",
            });

            base.Update(new ItemAdded
            {
                Id = 3,
                Name = "Fork",
            });
        }

        public void Rehydrate(ItemAdded e)
        { }
    }
}
