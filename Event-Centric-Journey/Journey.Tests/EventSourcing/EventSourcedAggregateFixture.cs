
using Journey.EventSourcing;
using Journey.EventSourcing.Handling;
using System;
using System.Collections.Generic;
using Xunit;
namespace Journey.Tests.EventSourcing.EventSourcedAggregateFixture
{
    public class GIVEN_aggregate
    {
        protected TestableAggregate sut; 

        public GIVEN_aggregate()
        {
            this.sut = new TestableAggregate(Guid.Empty);
        }

        [Fact]
        public void WHEN_receiving_command_once_THEN_handles()
        {
            var command = new Command1(Guid.Empty);

            Assert.Equal(0, this.sut.command1HandlingCount);

            this.sut.Handle(command);

            Assert.Equal(1, this.sut.command1HandlingCount);
        }

        /// <summary>
        /// This is why we need to ensure that an aggregate receives messages only once, or at least make 
        /// the aggregate consistent, event if the message where received twice.
        /// </summary>
        [Fact]
        public void WHEN_receiving_command_twice_THEN_handles_twice()
        {
            var command = new Command1(Guid.NewGuid());

            Assert.Equal(0, this.sut.command1HandlingCount);

            this.sut.Handle(command);
            this.sut.Handle(command);

            Assert.Equal(2, this.sut.command1HandlingCount);
        }
    }

    public class TestableAggregate : Journey.EventSourcing.EventSourced,
        IHandlerOf<Command1>
	{
        public int command1HandlingCount = 0;

        public TestableAggregate(Guid id)
            : base(id)
        {
        }

        public TestableAggregate(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            base.LoadFrom(history);
        }

        public void Handle(Command1 c)
        {
            base.Update(new Event1ForCommand1());
        }

        private void OnEvent1ForCommand1(Event1ForCommand1 e)
        {
            ++command1HandlingCount;
        }
    }

    public class Command1 : Journey.Messaging.ExternalCommand
    {
        public Command1(Guid id)
            : base(id)
        { }
    }

    public class Event1ForCommand1 : InternalVersionedEvent
    {
    }
}
