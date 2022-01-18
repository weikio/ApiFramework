using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiCache
    {
        byte[] Get(Endpoint endpoint, string key);
        Task<byte[]> GetAsync(Endpoint endpoint, string key, CancellationToken token = default);

        void Set(Endpoint endpoint, string key, byte[] value, ApiCacheEntryOptions options);
        Task SetAsync(Endpoint endpoint, string key, byte[] value, ApiCacheEntryOptions options, CancellationToken token = default);

        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration
        /// timeout (if any).
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="key">A string identifying the cached value.</param>
        void Refresh(Endpoint endpoint, string key);
        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration
        /// timeout (if any). 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="key">A string identifying the cached value.</param>
        /// <param name="token">
        /// Optional. The System.Threading.CancellationToken used to propagate notifications
        //  that the operation should be canceled.
        /// </param>
        /// <returns>The System.Threading.Tasks.Task that represents the asynchronous operation.</returns>
        Task RefreshAsync(Endpoint endpoint, string key, CancellationToken token = default);

        void Remove(Endpoint endpoint, string key);
        Task RemoveAsync(Endpoint endpoint, string key, CancellationToken token = default);
    }

    public class ApiCacheEntryOptions
    {
        /// <summary>
        /// Gets or sets an absolute expiration date for the cache entry.
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets an absolute expiration time, relative to now. 
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before
        //  it will be removed. This will not extend the entry lifetime beyond the absolute
        //  expiration (if set). 
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }
    }
}
