using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Samples.Cache
{

    public class HelloCacheApi
    {
        private readonly IEndpointCache _cache;

        public HelloCacheApi(IEndpointCache cache)
        {
            _cache = cache;
        }

        public string HelloCache(string name)
        {
            _cache.SetString("MyKey", name);
            var cacheValue = _cache.GetString("MyKey");
            return $"Hello {cacheValue} from cache";
        }
    }
}
