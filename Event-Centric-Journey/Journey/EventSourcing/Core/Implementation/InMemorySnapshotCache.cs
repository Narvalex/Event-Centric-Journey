using System.Collections.Specialized;
using System.Runtime.Caching;

namespace Infrastructure.CQRS.EventSourcing
{
    public class InMemorySnapshotCache : MemoryCache, ISnapshotCache
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="System.Runtime.Caching.MemoryCache"/> class.
        /// </summary>
        /// <param name="name">
        /// The name to use to look up configuration information.
        /// NOTE: It is not required
        /// for configuration information to exist for every name.If a matching configuration
        /// entry exists, the configuration information is used to configure the <see cref="System.Runtime.Caching.MemoryCache"/>
        /// instance. If a matching configuration entry does not exist, the name can
        /// be accessed through the <see cref="System.Runtime.Caching.MemoryCache.Name"/> property,
        /// because the specified name is associated with the <see cref="System.Runtime.Caching.MemoryCache"/>
        /// instance. For information about memory cache configuration, see <see cref="System.Runtime.Caching.Configuration.MemoryCacheElement"/>.
        /// </param>
        /// <param name="config">
        /// A collection of name/value pairs of configuration information to use for
        /// configuring the cache.
        /// </param>
        /// <exception cref="System.ArgumentNullException">name is null.</exception>
        /// <exception cref="System.ArgumentException">name is an empty string.</exception>
        /// <exception cref="System.ArgumentException">The string value "default" (case insensitive) is assigned to name. The value
        /// "default" cannot be assigned to a new System.Runtime.Caching.MemoryCache
        /// instance, because the value is reserved for use by the System.Runtime.Caching.MemoryCache.Default
        /// property.</exception>
        /// <exception cref="System.Configuration.ConfigurationException">A value in the config collection is invalid.name is null.</exception>
        /// <exception cref="System.ArgumentException">A name or value in the config parameter could not be parsed.</exception>
        public InMemorySnapshotCache(string name, NameValueCollection config = null)
            : base(name, config)
        { }
    }
}
