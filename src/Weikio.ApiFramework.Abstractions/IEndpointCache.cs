using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointCache
    {
        byte[] Get(string key);
        Task<byte[]> GetAsync(string key, CancellationToken token = default);

        void Set(string key, byte[] value, ApiCacheEntryOptions options);
        Task SetAsync(string key, byte[] value, ApiCacheEntryOptions options, CancellationToken token = default);

        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration
        /// timeout (if any).
        /// </summary>
        /// <param name="key">A string identifying the cached value.</param>
        void Refresh(string key);
        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration
        /// timeout (if any). 
        /// </summary>
        /// <param name="key">A string identifying the cached value.</param>
        /// <param name="token">
        /// Optional. The System.Threading.CancellationToken used to propagate notifications
        //  that the operation should be canceled.
        /// </param>
        /// <returns>The System.Threading.Tasks.Task that represents the asynchronous operation.</returns>
        Task RefreshAsync(string key, CancellationToken token = default);

        void Remove(string key);
        Task RemoveAsync(string key, CancellationToken token = default);
    }
}
