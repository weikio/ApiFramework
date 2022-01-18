using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weikio.ApiFramework.Abstractions;

namespace HelloWorld
{

    public class HelloWorldCacheApi
    {
        private readonly IEndpointCache _cache;
        private readonly TimeSpan _absoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);

        public HelloWorldCacheApi(IEndpointCache cache)
        {
            _cache = cache;
        }

        public string SetString(string key, string value)
        {
            _cache.SetString(key, value, _absoluteExpirationRelativeToNow);
            
            return _cache.GetString(key);
        }

        public string SetSlidingString(string key, string value, int slidingExpirationInSeconds)
        {
            _cache.SetString(key, value, new ApiCacheEntryOptions()
            { 
                SlidingExpiration = TimeSpan.FromSeconds(slidingExpirationInSeconds)
            });

            return _cache.GetString(key);
        }

        public string GetString(string key)
        {
            return _cache.GetString(key);
        }

        public string CreateString(string key, string value)
        {
            var cacheValue = _cache.GetOrCreateString(key, _absoluteExpirationRelativeToNow, () =>
            {
                return value;
            });
            
            return cacheValue;
        }

        public async Task<string> CreateAsyncronousStringAsync(string key, string value)
        {
            var cacheValue = await _cache.GetOrCreateStringAsync(key, _absoluteExpirationRelativeToNow, () =>
            {
                return Task.FromResult(value);
            });

            return cacheValue;
        }

        public void SetStringBytes(string key, string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            _cache.Set(key, valueBytes, _absoluteExpirationRelativeToNow);

            return;
        }

        public string GetStringFromBytes(string key)
        {
            var stringBytes = _cache.Get(key);
            if (stringBytes == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(stringBytes);
        }

        public void ItemRemove(string key)
        {
            _cache.Remove(key);
        }

        public void ItemRefresh(string key)
        {
            _cache.Refresh(key);
        }
    }
}
