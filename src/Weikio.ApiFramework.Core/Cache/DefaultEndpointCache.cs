using System;
using System.Collections.Generic;
using System.Text;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Cache
{
    public class DefaultEndpointCache : IEndpointCache
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
