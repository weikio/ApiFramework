using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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


        public DefaultApiCache(IDistributedCache distributedCache, IOptions<ApiCacheOptions> options)
        {
            _distributedCache = distributedCache;
            _apiCacheOptions = options.Value;
        }

        public string GetOrCreateString(Endpoint endpoint, string key, Func<string> getString)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var item = _distributedCache.GetString(cacheKey.ToString());
            if (item == null)
            {
                item = getString();
                _distributedCache.SetString(cacheKey, item);
            }
            return item;
        }

        public async Task<string> GetOrCreateStringAsync(Endpoint endpoint, string key, Func<Task<string>> getString)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var item = _distributedCache.GetString(cacheKey.ToString());
            if (item == null)
            {
                item = await getString();
                _distributedCache.SetString(cacheKey, item);
            }
            return item;
        }

        public byte[] GetOrCreateObject(Endpoint endpoint, string key, Func<byte[]> getObject)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var item = _distributedCache.Get(cacheKey.ToString());
            if (item == null)
            {
                item = getObject();
                _distributedCache.Set(cacheKey, item);
            }
            return item;
        }

        public async Task<byte[]> GetOrCreateObjectAsync(Endpoint endpoint, string key, Func<Task<byte[]>> getObject)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var item = _distributedCache.Get(cacheKey.ToString());
            if (item == null)
            {
                item = await getObject();
                _distributedCache.Set(cacheKey, item);
            }
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

        public byte[] GetObject(Endpoint endpoint, string key)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            return _distributedCache.Get(cacheKey.ToString());
        }

        public void SetObject(Endpoint endpoint, string key, byte[] value)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var options = GetEntryOptions();
            _distributedCache.Set(cacheKey, value, options);
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
