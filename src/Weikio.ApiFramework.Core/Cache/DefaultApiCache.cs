using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Cache
{
    public class DefaultApiCache : IApiCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ApiCacheOptions _cacheOptions;

        public DefaultApiCache(IDistributedCache distributedCache, IOptions<ApiCacheOptions> options)
        {
            _distributedCache = distributedCache;
            _cacheOptions = options.Value;
        }

        public string GetOrCreateString(string key, Func<string> getString)
        {
            var item = _distributedCache.GetString(key.ToString());
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
            return _distributedCache.GetString(key.ToString());
        }

        public void SetString(string key, string value)
        {
            var options = GetEntryOptions();
            _distributedCache.SetString(key, value, options);
        }

        private DistributedCacheEntryOptions GetEntryOptions()
        {
            var options = new DistributedCacheEntryOptions();
            if(_cacheOptions.ExpirationTimeInSeconds > 0)
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_cacheOptions.ExpirationTimeInSeconds);
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
