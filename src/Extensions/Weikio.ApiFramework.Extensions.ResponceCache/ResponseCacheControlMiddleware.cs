using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Weikio.ApiFramework.Core.Configuration;

namespace Weikio.ApiFramework.Extensions.ResponceCache
{
    public class ResponseCacheControlMiddleware
    {
        private readonly ILogger<ResponseCacheControlMiddleware> _logger;
        private readonly RequestDelegate _nextMiddleware;
        private readonly ResponceCacheOptions _options;
        private readonly ApiFrameworkOptions _apiFrameworkOptions;
        private readonly IMemoryCache _memoryCache;

        public ResponseCacheControlMiddleware(IOptions<ResponceCacheOptions> options, ILogger<ResponseCacheControlMiddleware> logger,
            IOptions<ApiFrameworkOptions> apiFrameworkOptions, IMemoryCache memoryCache, RequestDelegate nextMiddleware)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _nextMiddleware = nextMiddleware;
            _options = options.Value;
            _apiFrameworkOptions = apiFrameworkOptions.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpointMetadata();

            if (endpoint != null)
            {
                _logger.LogDebug("Processing response cache configuration for {Endpoint}", endpoint);

                var request = context.Request;
                var response = context.Response;

                var cacheEntry = GetRequestCacheConfiguration(request, endpoint);

                if (cacheEntry.Level == ResponseCacheConfigurationLevel.Undefined)
                {
                    _logger.LogDebug("No cache configuration found for {Endpoint}", endpoint.Route);
                }
                else
                {
                    response.GetTypedHeaders().CacheControl = cacheEntry.CacheControlHeader;

                    if (cacheEntry.Vary?.Any() == true)
                    {
                        response.Headers[HeaderNames.Vary] = cacheEntry.Vary;
                    }

                    _logger.LogDebug("Cache configuration found from level {ResponseCacheConfigurationLevel} for {Endpoint}. {CacheControl}, {Vary}.",
                        cacheEntry.Level, endpoint,
                        cacheEntry.CacheControlHeader?.ToString() ?? "", cacheEntry.Vary ?? Array.Empty<string>());
                }
            }

            await _nextMiddleware(context);
        }

        private ResponseCacheCachedEntry GetRequestCacheConfiguration(HttpRequest request, Abstractions.Endpoint endpoint)
        {
            return _memoryCache.GetOrCreate(request.Path.Value, entry =>
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

                        if (endpointCacheConfig.PathConfigurations.TryGetValue(path, out var cacheConfig))
                        {
                            return new ResponseCacheCachedEntry(ResponseCacheConfigurationLevel.Route,
                                new CacheControlHeaderValue() { Public = true, MaxAge = cacheConfig.MaxAge }, cacheConfig.Vary);
                        }

                        requestPathPartsQueue.Dequeue();
                    }

                    if (endpointCacheConfig.ResponseCacheConfiguration?.MaxAge != default)
                    {
                        return new ResponseCacheCachedEntry(ResponseCacheConfigurationLevel.Endpoint,
                            new CacheControlHeaderValue() { Public = true, MaxAge = endpointCacheConfig.ResponseCacheConfiguration.MaxAge },
                            endpointCacheConfig.ResponseCacheConfiguration.Vary);
                    }
                }

                if (_options.ResponseCacheConfiguration != null && _options.ResponseCacheConfiguration.MaxAge != default)
                {
                    return new ResponseCacheCachedEntry(ResponseCacheConfigurationLevel.Global,
                        new CacheControlHeaderValue() { Public = true, MaxAge = _options.ResponseCacheConfiguration.MaxAge },
                        _options.ResponseCacheConfiguration.Vary);
                }

                return new ResponseCacheCachedEntry(ResponseCacheConfigurationLevel.Undefined, null, null);
            });
        }

        private class ResponseCacheCachedEntry
        {
            public ResponseCacheConfigurationLevel Level { get; set; }
            public CacheControlHeaderValue CacheControlHeader { get; set; }
            public string[] Vary { get; set; }

            public ResponseCacheCachedEntry()
            {
            }

            public ResponseCacheCachedEntry(ResponseCacheConfigurationLevel level, CacheControlHeaderValue cacheControlHeader, string[] vary)
            {
                Level = level;
                CacheControlHeader = cacheControlHeader;
                Vary = vary;
            }
        }
    }
}
