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

        public byte[] Get(string key)
        {
            var endpoint = GetHttpEndpointMetadata();
            return _apiCache.Get(endpoint, key);
        }
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            var endpoint = GetHttpEndpointMetadata();
            return await _apiCache.GetAsync(endpoint, key, token);
        }

        public void Set(string key, byte[] value, ApiCacheEntryOptions options)
        {
            var endpoint = GetHttpEndpointMetadata();
            _apiCache.Set(endpoint, key, value, options);
        }

        public async Task SetAsync(string key, byte[] value, ApiCacheEntryOptions options, CancellationToken token = default)
        {
            var endpoint = GetHttpEndpointMetadata();
            await _apiCache.SetAsync(endpoint, key, value, options, token);
        }

        public void Refresh(string key)
        {
            var endpoint = GetHttpEndpointMetadata();
            _apiCache.Refresh(endpoint, key);
        }
        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            var endpoint = GetHttpEndpointMetadata();
            await _apiCache.RefreshAsync(endpoint, key, token);
        }

        public void Remove(string key)
        {
            var endpoint = GetHttpEndpointMetadata();
            _apiCache.Remove(endpoint, key);
        }
        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            var endpoint = GetHttpEndpointMetadata();
            await _apiCache.RemoveAsync(endpoint, key, token);
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
