using Infrastructure.CQRS.EventSourcing;
using Infrastructure.CQRS.Messaging;
using Infrastructure.CQRS.Messaging.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Infrastructure.CQRS.Utils.Testing
{
    public class EventSourcingTestHelper<T> where T : IEventSourced
    {
        private ICommandHandler handler;
        private readonly EventStoreStub store;
        private string expectedCorrelationId;

        public EventSourcingTestHelper()
        {
            this.Events = new List<ITraceableVersionedEvent>();
            this.store =
                new EventStoreStub((eventSourced, correlationId) =>
            {
                if (this.expectedCorrelationId != null)
                    Assert.Equal(this.expectedCorrelationId, correlationId);

                this.Events.AddRange(eventSourced.Events);
            });
        }

        public List<ITraceableVersionedEvent> Events { get; private set; }

        public IEventStore<T> Store { get { return this.store; } }

        public void Setup(ICommandHandler handler)
        {
            this.handler = handler;
        }

        public void Given(params ITraceableVersionedEvent[] history)
        {
            this.store.History.AddRange(history);
        }

        public void When(ICommand command)
        {
            this.expectedCorrelationId = command.Id.ToString();
            ((dynamic)this.handler).Handle((dynamic)command);
            this.expectedCorrelationId = null;
        }

        public void When(IEvent @event)
        {
            ((dynamic)this.handler).Handle((dynamic)@event);
        }

        public bool ThenContains<TEvent>() where TEvent : ITraceableVersionedEvent
        {
            return this.Events.Any(x => x.GetType() == typeof(TEvent));
        }

        public TEvent ThenHasSingle<TEvent>() where TEvent : ITraceableVersionedEvent
        {
            Assert.Equal(1, this.Events.Count);
            var @event = this.Events.Single();
            Assert.IsAssignableFrom<TEvent>(@event);
            return (TEvent)@event;
        }

        public TEvent ThenHasOne<TEvent>() where TEvent : ITraceableVersionedEvent
        {
            Assert.Equal(1, this.Events.OfType<TEvent>().Count());
            var @event = this.Events.OfType<TEvent>().Single();
            return @event;
        }

        private class EventStoreStub : IEventStore<T>
        {
            public readonly List<ITraceableVersionedEvent> History = new List<ITraceableVersionedEvent>();
            private readonly Action<T, string> onSave;
            private readonly Func<Guid, IEnumerable<ITraceableVersionedEvent>, T> entityFactory;

            internal EventStoreStub(Action<T, string> onSave)
            {
                this.onSave = onSave;
                var constructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<ITraceableVersionedEvent>) });
                if (constructor == null)
                {
                    throw new InvalidCastException(
                        "Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IVersionedEvent>)");
                }
                this.entityFactory = (id, events) => (T)constructor.Invoke(new object[] { id, events });
            }

            T IEventStore<T>.Find(Guid id)
            {
                var all = this.History.Where(x => x.SourceId == id).ToList();
                if (all.Count > 0)
                    return this.entityFactory.Invoke(id, all);

                return default(T);
            }

            void IEventStore<T>.Save(T eventSourced, Guid correlationId)
            {
                this.onSave(eventSourced, correlationId.ToString());
            }

            T IEventStore<T>.Get(Guid id)
            {
                var entity = ((IEventStore<T>)this).Find(id);
                if (EventStoreStub.Equals(entity, default(T)))
                    throw new EntityNotFoundException(id, "Test");

                return entity;
            }
        }
    }
}
