using System.Runtime.Caching;

namespace Journey.EventSourcing
{
    public interface IInMemoryRollingSnapshotProvider
    {
        /// <summary>
        /// Inserts a cache entry into the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="policy">An object that contains eviction details for the cache entry. This object 
        /// provides more options for eviction than a simple absolute expiration.</param>
        /// <param name="regionName">Optional. A named region in the cache to which the cache entry can be added, 
        /// if regions are implemented. The default value for the optional parameter is null.</param>

        void Set(string key, object value, CacheItemPolicy policy, string regionName = null);

        /// <summary>
        /// Gets the specified cache entry from the cache as an object,
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="regionName">Optional. A named region in the cache to which the cache entry was added, 
        /// if regions are implemented. The default value for the optional parameter is null.</param>
        /// <returns>The cache entry that is identified by key.</returns>
        object Get(string key, string regionName = null);

        /// <summary>
        /// Returns the total number of cache entries in the cache.
        /// </summary>
        /// <param name="regionName">A named region in the cache to which a cache entry was added. Do not pass
        /// a value for this parameter. This parameter is null by default, because the
        /// System.Runtime.Caching.MemoryCache class does not implement regions.</param>
        /// <returns>The number of entries in the cache.</returns>
        long GetCount(string regionName = null);
    }
}
