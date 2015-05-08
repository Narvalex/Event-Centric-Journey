using System.Data.Entity;
using System.Linq;

namespace Journey.Utils
{
    /// <summary>
    /// Usability extensions for DbContexts.
    /// </summary>
    public static class DbContextExtensions
    {
        public static void AddToUnityOfWork<T>(this DbContext context, T entity) where T : class
        {
            var entry = context.Entry(entity);

            if (entry.State == EntityState.Detached)
                context.Set<T>().Add(entity);
        }

        public static IQueryable<T> Query<T>(this DbContext context) where T : class
        {
            return context.Set<T>();
        }
    }
}
