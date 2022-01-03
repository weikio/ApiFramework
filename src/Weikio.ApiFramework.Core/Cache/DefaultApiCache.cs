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
        private readonly ICacheOptions _cacheOptions;
        private readonly ApiCacheOptions _apiCacheOptions;


        public DefaultApiCache(IDistributedCache distributedCache, 
            IOptions<ApiCacheOptions> options, ICacheOptions cacheOptions)
        {
            _distributedCache = distributedCache;
            _cacheOptions = cacheOptions;
            _apiCacheOptions = options.Value;
        }

        private string GetKey(string key)
        {
            return _cacheOptions.KeyFunc(key);
        }

        public string GetOrCreateString(string key, Func<string> getString)
        {
            var cacheKey = GetKey(key);
            var item = _distributedCache.GetString(cacheKey.ToString());
            if (item != null)
            {
                return item;
            }
            item = getString();
            _distributedCache.SetString(key, item);
            return item;
        }

        public string GetString(string key)
        {
            var cacheKey = GetKey(key);
            return _distributedCache.GetString(cacheKey.ToString());
        }

        public void SetString(string key, string value)
        {
            var cacheKey = GetKey(key);
            var options = GetEntryOptions();
            _distributedCache.SetString(cacheKey, value, options);
        }

        private DistributedCacheEntryOptions GetEntryOptions()
        {
            var options = new DistributedCacheEntryOptions();
            if(_apiCacheOptions.ExpirationTimeInSeconds > 0)
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_apiCacheOptions.ExpirationTimeInSeconds);
            }
            return options;
        }

        public void SetObject(string key, object value)
        {
            
        }

        public object GetObject(string key)
        {
            return default(object);
        }
    }
}
