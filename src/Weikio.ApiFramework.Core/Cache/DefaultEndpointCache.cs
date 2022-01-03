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

        public DefaultEndpointCache(IApiCache apiCache)
        {
            _apiCache = apiCache;
        }

        public object GetObject(string key)
        {
            return default(object);
        }

        public string GetString(string key)
        {
            return _apiCache.GetString(key);
        }

        public void SetObject(string key, object value)
        {

        }

        public void SetString(string key, string value)
        {
            _apiCache.SetString(key, value);
        }
    }
}
