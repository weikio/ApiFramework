using System;
using System.Collections.Generic;
using System.Text;
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


        public DefaultApiCache(IDistributedCache distributedCache, IOptions<ApiCacheOptions> options)
        {
            _distributedCache = distributedCache;
            _apiCacheOptions = options.Value;
        }

        public string GetOrCreateString(Endpoint endpoint, string key, Func<string> getString)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var item = _distributedCache.GetString(cacheKey.ToString());
            if (item != null)
            {
                return item;
            }
            item = getString();
            _distributedCache.SetString(key, item);
            return item;
        }

        public string GetString(Endpoint endpoint, string key)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            return _distributedCache.GetString(cacheKey.ToString());
        }

        public void SetString(Endpoint endpoint, string key, string value)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var options = GetEntryOptions();
            _distributedCache.SetString(cacheKey, value, options);
        }

        public void SetObject(Endpoint endpoint, string key, object value)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var options = GetEntryOptions();
            //_distributedCache.SetString(cacheKey, value, options);
        }

        public object GetObject(Endpoint endpoint, string key)
        {
            return default(object);
        }

        private DistributedCacheEntryOptions GetEntryOptions()
        {
            var options = new DistributedCacheEntryOptions();
            if (_apiCacheOptions.ExpirationTimeInSeconds > 0)
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_apiCacheOptions.ExpirationTimeInSeconds);
            }
            return options;
        }
    }
}
