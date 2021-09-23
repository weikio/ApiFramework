using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Weikio.ApiFramework.Core.Configuration;

namespace Weikio.ApiFramework.ResponceCache
{
    public class ResponseCacheControlMiddleware
    {
        private readonly ILogger<ResponseCacheControlMiddleware> _logger;
        private readonly RequestDelegate _nextMiddleware;
        private readonly ResponceCacheOptions _options;
        private readonly ApiFrameworkOptions _apiFrameworkOptions;
        private static readonly ConcurrentDictionary<string, ResponseCacheCachedEntry> _memoryCache = new ConcurrentDictionary<string, ResponseCacheCachedEntry>();

        public ResponseCacheControlMiddleware(IOptions<ResponceCacheOptions> options, ILogger<ResponseCacheControlMiddleware> logger,
            IOptions<ApiFrameworkOptions> apiFrameworkOptions, RequestDelegate nextMiddleware)
        {
            _logger = logger;
            _nextMiddleware = nextMiddleware;
            _options = options.Value;
            _apiFrameworkOptions = apiFrameworkOptions.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpointMetadata();

            if (endpoint != null)
            {
                _logger.LogTrace("Processing response cache configuration for {Endpoint}", endpoint);

                var request = context.Request;
                var response = context.Response;

                var cacheEntry = GetRequestCacheConfiguration(request, endpoint);

                if (cacheEntry.Level == ResponseCacheConfigurationLevel.Undefined)
                {
                    _logger.LogTrace("No cache configuration found for {Endpoint}", endpoint.Route);
                }
                else
                {
                    response.GetTypedHeaders().CacheControl = cacheEntry.CacheControlHeader;

                    if (cacheEntry.Vary?.Any() == true)
                    {
                        response.Headers[HeaderNames.Vary] = cacheEntry.Vary;
                    }

                    if (cacheEntry.VaryByQueryKeys?.Any() == true)
                    {
                        var responseCachingFeature = context.Features.Get<IResponseCachingFeature>();

                        if (responseCachingFeature != null)
                        {
                            responseCachingFeature.VaryByQueryKeys = cacheEntry.VaryByQueryKeys;
                        }
                    }

                    _logger.LogDebug("Cache configuration found from level {ResponseCacheConfigurationLevel} for {Endpoint}. {CacheControl}, {Vary}, {VaryByQueryKeys}.",
                        cacheEntry.Level, endpoint,
                        cacheEntry.CacheControlHeader?.ToString() ?? "", cacheEntry.Vary ?? Array.Empty<string>(), cacheEntry.VaryByQueryKeys ?? Array.Empty<string>());
                }
            }

            await _nextMiddleware(context);
        }

        private ResponseCacheCachedEntry GetRequestCacheConfiguration(HttpRequest request, Abstractions.Endpoint endpoint)
        {
            return _memoryCache.GetOrAdd(request.Path.Value, entry =>
            {
                // Request example: /api/HelloWorld/TimeTest where /api is the Api Framework's base address and /HelloWorld is the endpoint's route.
                // As the route based caching is defined inside the Endpoint's configuration, we want to match the cache routes against the routes inside the endpoint.
                // So instead of checking if we find a cache route for /api/HelloWorld/TimeTest, we check if we find one for /TimeTest
                var requestPathParts = request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
                requestPathParts.Remove(endpoint.Route.Replace("/", ""));

                var containsApiAddressBase = requestPathParts.Any() && string.Equals(requestPathParts.First(),
                    _apiFrameworkOptions.ApiAddressBase.Replace("/", ""),
                    StringComparison.InvariantCultureIgnoreCase);

                if (containsApiAddressBase)
                {
                    requestPathParts.Remove(_apiFrameworkOptions.ApiAddressBase.Replace("/", ""));
                }

                var requestPathPartsQueue = new Queue<string>(requestPathParts);

                if (_options.EndpointResponceCacheConfigurations?.Any() == true &&
                    _options.EndpointResponceCacheConfigurations.TryGetValue(endpoint.Route, out var endpointCacheConfig))
                {
                    while (requestPathPartsQueue.Any())
                    {
                        var path = new PathString("/" + string.Join('/', requestPathPartsQueue));

                        if (endpointCacheConfig.PathConfigurations.TryGetValue(path, out var cacheConfig) && cacheConfig.MaxAge > default(TimeSpan))
                        {
                            return new ResponseCacheCachedEntry(ResponseCacheConfigurationLevel.Route,
                                new CacheControlHeaderValue() { Public = true, MaxAge = cacheConfig.MaxAge }, cacheConfig.Vary, cacheConfig.VaryByQueryKeys);
                        }

                        requestPathPartsQueue.Dequeue();
                    }

                    if (endpointCacheConfig.ResponseCacheConfiguration != null && endpointCacheConfig.ResponseCacheConfiguration.MaxAge > default(TimeSpan))
                    {
                        return new ResponseCacheCachedEntry(ResponseCacheConfigurationLevel.Endpoint,
                            new CacheControlHeaderValue() { Public = true, MaxAge = endpointCacheConfig.ResponseCacheConfiguration.MaxAge },
                            endpointCacheConfig.ResponseCacheConfiguration.Vary, endpointCacheConfig.ResponseCacheConfiguration.VaryByQueryKeys);
                    }
                }

                if (_options.ResponseCacheConfiguration != null && _options.ResponseCacheConfiguration.MaxAge > default(TimeSpan))
                {
                    return new ResponseCacheCachedEntry(ResponseCacheConfigurationLevel.Global,
                        new CacheControlHeaderValue() { Public = true, MaxAge = _options.ResponseCacheConfiguration.MaxAge },
                        _options.ResponseCacheConfiguration.Vary, _options.ResponseCacheConfiguration.VaryByQueryKeys);
                }

                return new ResponseCacheCachedEntry(ResponseCacheConfigurationLevel.Undefined, null, null, null);
            });
        }

        private class ResponseCacheCachedEntry
        {
            public ResponseCacheConfigurationLevel Level { get; set; }
            public CacheControlHeaderValue CacheControlHeader { get; set; }
            public string[] Vary { get; set; }
            public string[] VaryByQueryKeys { get; }

            public ResponseCacheCachedEntry()
            {
            }

            public ResponseCacheCachedEntry(ResponseCacheConfigurationLevel level, CacheControlHeaderValue cacheControlHeader, string[] vary, string[] varyByQueryKeys)
            {
                Level = level;
                CacheControlHeader = cacheControlHeader;
                Vary = vary;
                VaryByQueryKeys = varyByQueryKeys;
            }
        }
    }
}
