using Journey.Worker;
using Journey.Messaging;
using Journey.Messaging.Processing;
using System;
using System.Diagnostics;
using Xunit;

namespace Journey.Tests.Integration.Messaging.EventSubscriptionFixture
{
    public class GIVEN_dispatcher_with_multiple_handlers
    {
        private AsynchronousEventDispatcher sut = new AsynchronousEventDispatcher(new ConsoleTracer());
        private TestableAggregateHandler handler = new TestableAggregateHandler();

        public GIVEN_dispatcher_with_multiple_handlers()
        {

            this.sut.Register(handler as IEventHandler<EventA>);
            this.sut.Register(handler as IEventHandler<EventB>);
            this.sut.Register(handler as IEventHandler<EventC>);

        }

        [Fact]
        public void WHEN_dispatching_an_event_with_multiple_registered_handlers_THEN_invokes_handlers()
        {
            var @event = new EventA();

            this.sut.DispatchMessage(@event, "message", "correlation", "");
        }

        [Fact]
        public void WHEN_dispatching_an_event_with_single_registered_handler_THEN_invokes_handler()
        {
            var @event = new EventB();

            this.sut.DispatchMessage(@event, "message", "correlation", "");
        }

        [Fact]
        public void WHEN_dispatching_an_event_with_no_registered_handler_THEN_does_nothing()
        {
            var @event = new EventC();

            this.sut.DispatchMessage(@event, "message", "correlation", "");
        }
    }

    public class EventA : IEvent
    {
        public Guid SourceId { get; set; }

        public DateTime TimeStamp { get; set; }

        public DateTime SourceTimeStamp { get; set; }

        public DateTime CreationDate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public class EventB : IEvent
    {
        public Guid SourceId { get; set; }

        public DateTime TimeStamp { get; set; }

        public DateTime SourceTimeStamp { get; set; }

        public DateTime CreationDate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public class EventC : IEvent
    {
        public Guid SourceId { get; set; }

        public DateTime TimeStamp { get; set; }

        public DateTime SourceTimeStamp { get; set; }

        public DateTime CreationDate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

    public class TestableAggregateHandler :
    IEventHandler<EventA>,
    IEventHandler<EventB>,
    IEventHandler<EventC>
    {
        public void Handle(EventA e)
        {
            this.TraceHandling(e);
        }

        public void Handle(EventB e)
        {
            this.TraceHandling(e);
        }

        public void Handle(EventC e)
        {
            this.TraceHandling(e);
        }

        private void TraceHandling(object @event)
        {
            Trace.WriteLine("Handling " + @event.GetType().ToString());
        }
    }
}
