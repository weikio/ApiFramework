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
        
        public byte[] GetData(Endpoint endpoint, string key)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);

            return _distributedCache.Get(cacheKey.ToString());
        }

        public async Task<byte[]> GetDataAsync(Endpoint endpoint, string key, CancellationToken token = default)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);

            return await _distributedCache.GetAsync(cacheKey.ToString(), token);
        }

        public void SetData(Endpoint endpoint, string key, byte[] value)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var options = GetEntryOptions();
            _distributedCache.Set(cacheKey, value, options);
        }

        public async Task SetDataAsync(Endpoint endpoint, string key, byte[] value, CancellationToken token = default)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var options = GetEntryOptions();
            await _distributedCache.SetAsync(cacheKey, value, options, token);
        }

        private DistributedCacheEntryOptions GetEntryOptions()
        {
            var options = new DistributedCacheEntryOptions();
            if (_apiCacheOptions.ExpirationTime.TotalSeconds > 0)
            {
                options.AbsoluteExpirationRelativeToNow = _apiCacheOptions.ExpirationTime;
            }

            return options;
        }
    }
}
