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
                _distributedCache.SetString(key, item);
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
                _distributedCache.SetString(key, item);
            }
            return item;
        }

        public object GetOrCreateObject(Endpoint endpoint, string key, Func<object> getObject)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var item = _distributedCache.Get(cacheKey.ToString());
            if (item == null)
            {
                item = ObjectToByteArray(getObject());
                _distributedCache.Set(key, item);
            }
            return ByteArrayToObject(item);
        }

        public async Task<object> GetOrCreateObjectAsync(Endpoint endpoint, string key, Func<Task<object>> getObject)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var item = _distributedCache.Get(cacheKey.ToString());
            if (item == null)
            {
                item = ObjectToByteArray(await getObject());
                _distributedCache.Set(key, item);
            }
            return ByteArrayToObject(item);
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

        public object GetObject(Endpoint endpoint, string key)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            return ByteArrayToObject(_distributedCache.Get(cacheKey.ToString()));
        }

        public void SetObject(Endpoint endpoint, string key, object value)
        {
            var cacheKey = _apiCacheOptions.GetKey(endpoint, _serviceProvider, key);
            var options = GetEntryOptions();
            _distributedCache.Set(cacheKey, ObjectToByteArray(value), options);
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

        private byte[] ObjectToByteArray(object obj)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}
