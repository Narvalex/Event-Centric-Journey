using System.Data.Entity;
using System.Linq;

namespace Infrastructure.CQRS.EventSourcing.ReadModeling
{
    public class ReadModelDbContext : DbContext
    {
        public ReadModelDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        { }

        /// <summary>
        /// Verifica si la fila de la tabla ya esta actualizada, si no esta actualizada 
        /// entonces puede recibir actualizacion via eventos
        /// </summary>
        public bool ReadModelIsUpToDate<T, E>(E @event) 
            where T : TraceableEventSourcedEntity
            where E : ITraceableVersionedEvent
        {
            return this.Set<T>().Where(entity => 
                entity.AggregateId == @event.SourceId && 
                entity.AggregateType == @event.AggregateType && 
                entity.Version >= @event.Version)
                .Any();
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return this.Set<T>();
        }

        public void AddToUnitOfWork<T>(T entity) where T : class
        {
            var entry = this.Entry(entity);

            if (entry.State == EntityState.Detached)
                this.Set<T>().Add(entity);
        }
    }
}
