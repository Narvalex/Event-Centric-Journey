using Journey.Utils;
using System;
using System.Linq.Expressions;

namespace Journey.Messaging.Logging
{
    internal static class MessageLogQueryCriteriaExtensions
    {
        public static Expression<Func<MessageLogEntity, bool>> ToExpression(this EventLogQueryCriteria criteria)
        {
            // The full Where clause being built.
            Expression<Func<MessageLogEntity, bool>> expression = null;

            foreach (var asembly in criteria.AssemblyNames)
            {
                var value = asembly;
                if (expression == null)
                    expression = e => e.AssemblyName == value;
                else
                    expression = expression.Or(e => e.AssemblyName == value);
            }

            // The current criteria filter being processed (i.e. FullName).
            Expression<Func<MessageLogEntity, bool>> filter = null;
            foreach (var item in criteria.FullNames)
            {
                var value = item;
                if (filter == null)
                    filter = e => e.FullName == value;
                else
                    filter = filter.Or(e => e.FullName == value);
            }

            if (filter != null)
            {
                expression = (expression == null) ? filter : expression.And(filter);
                filter = null;
            }

            foreach (var item in criteria.Namespaces)
            {
                var value = item;
                if (filter == null)
                    filter = e => e.Namespace == value;
                else
                    filter = filter.Or(e => e.Namespace == value);
            }

            if (filter != null)
            {
                expression = (expression == null) ? filter : expression.And(filter);
                filter = null;
            }

            foreach (var item in criteria.SourceIds)
            {
                var value = item;
                if (filter == null)
                    filter = e => e.SourceId == value;
                else
                    filter = filter.Or(e => e.SourceId == value);
            }

            if (filter != null)
            {
                expression = (expression == null) ? filter : expression.And(filter);
                filter = null;
            }

            foreach (var item in criteria.SourceTypes)
            {
                var value = item;
                if (filter == null)
                    filter = e => e.SourceType == value;
                else
                    filter = filter.Or(e => e.SourceType == value);
            }

            if (filter != null)
            {
                expression = (expression == null) ? filter : expression.And(filter);
                filter = null;
            }

            foreach (var item in criteria.TypeNames)
            {
                var value = item;
                if (filter == null)
                    filter = e => e.TypeName == value;
                else
                    filter = filter.Or(e => e.TypeName == value);
            }

            if (filter != null)
            {
                expression = (expression == null) ? filter : expression.And(filter);
                filter = null;
            }

            if (criteria.EndDate.HasValue)
            {
                var creationDateFilter = criteria.EndDate.Value.ToString("o");
                filter = e => e.CreationDate.CompareTo(creationDateFilter) <= 0;

                expression = (expression == null) ? filter : expression.And(filter);
                filter = null;
            }

            return expression;
        }
    }
}
