using Journey.Worker;
using Journey.Messaging;
using Journey.Messaging.Processing;
using Moq;
using System;
using Xunit;

namespace Journey.Tests.Messaging.MessageDispatcherFixture
{
    public class GIVEN_empty_dispatcher
    {
        private AsynchronousEventDispatcher sut;

        public GIVEN_empty_dispatcher()
        {
            this.sut = new AsynchronousEventDispatcher(new ConsoleTracer());
        }

        [Fact]
        public void WHEN_dispatching_an_event_THEN_does_not_nothing()
        {
            var @event = new EventC();
            this.sut.DispatchMessage(@event, "message", "correlationId", "");
        }
    }

    public class GIVEN_dispatcher_with_handler
    {
        private AsynchronousEventDispatcher sut = new AsynchronousEventDispatcher(new ConsoleTracer());
        private Mock<IEventHandler> handlerMock = new Mock<IEventHandler>();

        public GIVEN_dispatcher_with_handler()
        {
            this.handlerMock.As<IEventHandler<EventA>>();

            this.sut.Register(this.handlerMock.Object);
        }

        [Fact]
        public void WHEN_dispatching_an_event_with_registered_handler_THEN_invokes_handler() 
        {
            var @event = new EventA();

            this.sut.DispatchMessage(@event, "message", "correlation", "");

            this.handlerMock.As<IEventHandler<EventA>>().Verify(h => h.Handle(@event), Times.Once);
        }

        [Fact]
        public void WHEN_dispatching_an_event_with_no_registered_handler_THEN_does_nothing()
        {
            var @event = new EventC();

            this.sut.DispatchMessage(@event, "message", "correlation", "");
        }
        
    }

    public class GIVEN_dispatcher_with_multiple_handlers
    {
        private AsynchronousEventDispatcher sut = new AsynchronousEventDispatcher(new ConsoleTracer());
        private Mock<IEventHandler> handler1Mock = new Mock<IEventHandler>();
        private Mock<IEventHandler> handler2Mock = new Mock<IEventHandler>();

        public GIVEN_dispatcher_with_multiple_handlers()
        {
            this.handler1Mock.As<IEventHandler<EventA>>();
            this.handler1Mock.As<IEventHandler<EventB>>();

            this.sut.Register(this.handler1Mock.Object);

            this.handler2Mock.As<IEventHandler<EventA>>();

            this.sut.Register(this.handler2Mock.Object);
        }

        [Fact]
        public void WHEN_dispatching_an_event_with_multiple_registered_handlers_THEN_invokes_handlers()
        {
            var @event = new EventA();

            this.sut.DispatchMessage(@event, "message", "correlation", "");

            this.handler1Mock.As<IEventHandler<EventA>>()
                .Verify(h => h.Handle(@event), Times.Once());

            this.handler2Mock.As<IEventHandler<EventA>>()
                .Verify(h => h.Handle(@event), Times.Once());
        }

        [Fact]
        public void WHEN_dispatching_an_event_with_single_registered_handler_THEN_invokes_handler()
        {
            var @event = new EventB();

            this.sut.DispatchMessage(@event, "message", "correlation", "");

            this.handler1Mock.As<IEventHandler<EventB>>().Verify(h => h.Handle(@event), Times.Once());
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


        public bool IsExternal
        {
            get { throw new NotImplementedException(); }
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


        public bool IsExternal
        {
            get { throw new NotImplementedException(); }
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


        public bool IsExternal
        {
            get { throw new NotImplementedException(); }
        }
    }
}
