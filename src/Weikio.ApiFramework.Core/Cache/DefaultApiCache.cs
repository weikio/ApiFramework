using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Cache
{
    internal class DefaultApiCache : IApiCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApiCacheOptions _apiCacheOptions;

        public DefaultApiCache(IDistributedCache distributedCache, IServiceProvider serviceProvider, IOptions<ApiCacheOptions> options)
        {
            _distributedCache = distributedCache;
            _serviceProvider = serviceProvider;
            _apiCacheOptions = options.Value;
        }
        
        public byte[] Get(Endpoint endpoint, string key)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);

            return _distributedCache.Get(cacheKey.ToString());
        }

        public async Task<byte[]> GetAsync(Endpoint endpoint, string key, CancellationToken token = default)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);

            return await _distributedCache.GetAsync(cacheKey.ToString(), token);
        }

        public void Set(Endpoint endpoint, string key, byte[] value, ApiCacheEntryOptions options)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var entryOptions = GetEntryOptions(options);
            _distributedCache.Set(cacheKey, value, entryOptions);
        }

        public async Task SetAsync(Endpoint endpoint, string key, byte[] value, ApiCacheEntryOptions options, CancellationToken token = default)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var entryOptions = GetEntryOptions(options);
            await _distributedCache.SetAsync(cacheKey, value, entryOptions, token);
        }

        public void Refresh(Endpoint endpoint, string key)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);

            _distributedCache.Refresh(cacheKey.ToString());
        }

        public async Task RefreshAsync(Endpoint endpoint, string key, CancellationToken token = default)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);

            await _distributedCache.RefreshAsync(cacheKey.ToString(), token);
        }

        public void Remove(Endpoint endpoint, string key)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);

            _distributedCache.Remove(cacheKey.ToString());
        }

        public async Task RemoveAsync(Endpoint endpoint, string key, CancellationToken token = default)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);

            await _distributedCache.RemoveAsync(cacheKey.ToString(), token);
        }

        private DistributedCacheEntryOptions GetEntryOptions(ApiCacheEntryOptions options)
        {
            return new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration
            };
        }
    }
}
