using System;
using System.Collections.Generic;
using System.Text;
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
