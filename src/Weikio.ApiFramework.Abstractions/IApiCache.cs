using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IApiCache
    {
        byte[] GetData(Endpoint endpoint, string key);
        Task<byte[]> GetDataAsync(Endpoint endpoint, string key, CancellationToken token = default);

        void SetData(Endpoint endpoint, string key, byte[] value, ApiCacheEntryOptions options);
        Task SetDataAsync(Endpoint endpoint, string key, byte[] value, ApiCacheEntryOptions options, CancellationToken token = default);
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
