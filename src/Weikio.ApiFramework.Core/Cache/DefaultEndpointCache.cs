using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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

        public byte[] GetData(string key)
        {
            var endpoint = GetHttpEndpointMetadata();
            return _apiCache.GetData(endpoint, key);
        }
        public async Task<byte[]> GetDataAsync(string key, CancellationToken token = default)
        {
            var endpoint = GetHttpEndpointMetadata();
            return await _apiCache.GetDataAsync(endpoint, key, token);
        }

        public void SetData(string key, byte[] value, ApiCacheEntryOptions options)
        {
            var endpoint = GetHttpEndpointMetadata();
            _apiCache.SetData(endpoint, key, value, options);
        }

        public async Task SetDataAsync(string key, byte[] value, ApiCacheEntryOptions options, CancellationToken token = default)
        {
            var endpoint = GetHttpEndpointMetadata();
            await _apiCache.SetDataAsync(endpoint, key, value, options, token);
        }

        private Abstractions.Endpoint GetHttpEndpointMetadata()
        {
            var httpContext = _contextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new InvalidOperationException("Endpoint metadata cannot be retrieved without HttpContext.");
            }

            var endpointMetadata = httpContext.GetEndpointMetadata();

            if (endpointMetadata == null)
            {
                throw new InvalidOperationException("Endpoint metadata cannot be found from HttpContext.");
            }

            return endpointMetadata;
        }
    }
}
