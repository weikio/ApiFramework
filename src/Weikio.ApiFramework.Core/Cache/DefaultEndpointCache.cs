using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.AspNetCore;

namespace Weikio.ApiFramework.Core.Cache
{
    internal class DefaultEndpointCache : IEndpointCache
    {
        private readonly IApiCache _apiCache;
        private readonly IHttpContextAccessor _contextAccessor;

        public DefaultEndpointCache(IApiCache apiCache, IHttpContextAccessor contextAccessor)
        {
            _apiCache = apiCache;
            _contextAccessor = contextAccessor;
        }

        public string GetOrCreateString(string key, Func<string> getString)
        {
            var endpoint = _contextAccessor.HttpContext.GetEndpointMetadata();
            return _apiCache.GetOrCreateString(endpoint, key, getString);
        }

        public async Task<string> GetOrCreateStringAsync(string key, Func<Task<string>> getString)
        {
            var endpoint = _contextAccessor.HttpContext.GetEndpointMetadata();
            return await _apiCache.GetOrCreateStringAsync(endpoint, key, getString);
        }

        public object GetOrCreateObject(string key, Func<object> getObject)
        {
            var endpoint = _contextAccessor.HttpContext.GetEndpointMetadata();
            return _apiCache.GetOrCreateObject(endpoint, key, getObject);
        }

        public async Task<object> GetOrCreateObjectAsync(string key, Func<Task<object>> getObject)
        {
            var endpoint = _contextAccessor.HttpContext.GetEndpointMetadata();
            return await _apiCache.GetOrCreateObjectAsync(endpoint, key, getObject);
        }

        public object GetObject(string key)
        {
            var endpoint = _contextAccessor.HttpContext.GetEndpointMetadata();
            return _apiCache.GetObject(endpoint, key);
        }

        public string GetString(string key)
        {
            var endpoint = _contextAccessor.HttpContext.GetEndpointMetadata();
            return _apiCache.GetString(endpoint, key);
        }

        public void SetObject(string key, object value)
        {
            var endpoint = _contextAccessor.HttpContext.GetEndpointMetadata();
            _apiCache.SetObject(endpoint, key, value);
        }

        public void SetString(string key, string value)
        {
            var endpoint = _contextAccessor.HttpContext.GetEndpointMetadata();
            _apiCache.SetString(endpoint, key, value);
        }
    }
}
