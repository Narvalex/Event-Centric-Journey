using System;
using System.Collections;
using System.Collections.Generic;

namespace Journey.Messaging.Logging
{
    /// <summary>
    /// Provides usability overloads and fluent querying API for the event log.
    /// </summary>
    public static class EventLogExtensions
    {
        /// <summary>
        /// Reads all events in the log.
        /// </summary>
        public static IEnumerable<IEvent> ReadAll(this IEventLogReader log)
        {
            return log.Query(new EventLogQueryCriteria());
        }

        /// <summary>
        /// Queries the specified log using a fluent API.
        /// </summary>
        public static IEventQuery Query(this IEventLogReader log)
        {
            return new EventQuery(log);
        }

        /// <summary>
        /// Provides a fluent API to filter events from the event log. 
        /// </summary>
        public partial interface IEventQuery : IEnumerable<IEvent>
        {
            /// <summary>
            /// Executes the query built using the fluent API 
            /// against the underlying store.
            /// </summary>
            IEnumerable<IEvent> Execute();

            /// <summary>
            /// Filters events with a matching type name metadata.
            /// </summary>
            IEventQuery WithTypeName(string typeName);

            /// <summary>
            /// Filters events with a matching full type name metadata.
            /// </summary>
            IEventQuery WithFullName(string fullName);

            /// <summary>
            /// Filters events with a matching assembly name metadata.
            /// </summary>
            IEventQuery FromAssembly(string assemblyName);

            /// <summary>
            /// Filters events with a matching namespace metadata.
            /// </summary>
            IEventQuery FromNamespace(string @namespace);

            /// <summary>
            /// Filters events with a matching source type name metadata.
            /// </summary>
            IEventQuery FromSource(string sourceType);

            /// <summary>
            /// Filters events that occurred until the specified date.
            /// </summary>
            IEventQuery Until(DateTime endDate);
        }

        /// <summary>
        /// Implements the criteria builder fluent API.
        /// </summary>
        private class EventQuery : IEventQuery, IEnumerable<IEvent>
        {
            private IEventLogReader log;
            private EventLogQueryCriteria criteria = new EventLogQueryCriteria();

            public EventQuery(IEventLogReader log)
            {
                this.log = log;
            }

            public IEnumerable<IEvent> Execute()
            {
                return log.Query(this.criteria);
            }

            public IEnumerator<IEvent> GetEnumerator()
            {
                return this.Execute().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public IEventQuery WithTypeName(string typeName)
            {
                this.criteria.TypeNames.Add(typeName);
                return this;
            }

            public IEventQuery WithFullName(string fullName)
            {
                this.criteria.FullNames.Add(fullName);
                return this;
            }

            public IEventQuery FromAssembly(string assemblyName)
            {
                this.criteria.AssemblyNames.Add(assemblyName);
                return this;
            }

            public IEventQuery FromNamespace(string @namespace)
            {
                this.criteria.Namespaces.Add(@namespace);
                return this;
            }

            public IEventQuery FromSource(string sourceType)
            {
                this.criteria.SourceTypes.Add(sourceType);
                return this;
            }

            public IEventQuery Until(DateTime endDate)
            {
                this.criteria.EndDate = endDate;
                return this;
            }
        }
    }

}
