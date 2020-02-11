using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Extensions.ResponceCache
{
    public class ResponseCacheControlMiddleware
    {
        private readonly ILogger<ResponseCacheControlMiddleware> _logger;
        private readonly RequestDelegate _nextMiddleware;
        private readonly ResponceCacheOptions _options;
        private readonly ApiFrameworkOptions _apiFrameworkOptions;

        public ResponseCacheControlMiddleware(IOptions<ResponceCacheOptions> options, ILogger<ResponseCacheControlMiddleware> logger, IOptions<ApiFrameworkOptions> apiFrameworkOptions, RequestDelegate nextMiddleware)
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
                var request = context.Request;
                var response = context.Response;

                var requestPathParts = request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

                requestPathParts.Remove(endpoint.Route.Replace("/", ""));

                if (requestPathParts.Any() && string.Equals(requestPathParts.First(), _apiFrameworkOptions.ApiAddressBase.Replace("/", ""),
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    requestPathParts.Remove(_apiFrameworkOptions.ApiAddressBase.Replace("/", ""));
                }

                var requestPathPartsQueue = new Queue<string>(requestPathParts);

                var cacheAdded = false;

                if (_options.EndpointResponceCacheConfigurations?.Any() == true && _options.EndpointResponceCacheConfigurations.TryGetValue(endpoint.Route, out var endpointCacheConfig))
                {
                    while (requestPathPartsQueue.Any())
                    {
                        var path = new PathString("/" + string.Join('/', requestPathPartsQueue));

                        if (endpointCacheConfig.PathConfigurations.TryGetValue(path, out var cacheConfig))
                        {
                            response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue() { Public = true, MaxAge = cacheConfig.MaxAge };

                            if (cacheConfig.Vary.Any())
                            {
                                response.Headers[HeaderNames.Vary] = cacheConfig.Vary;
                            }

                            cacheAdded = true;

                            break;
                        }

                        requestPathPartsQueue.Dequeue();
                    }

                    if (!cacheAdded && endpointCacheConfig.ResponseCacheConfiguration?.MaxAge != default)
                    {
                        response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue() { Public = true, MaxAge = endpointCacheConfig.ResponseCacheConfiguration.MaxAge };

                        if (endpointCacheConfig.ResponseCacheConfiguration?.Vary?.Any() == true)
                        {
                            response.Headers[HeaderNames.Vary] = endpointCacheConfig.ResponseCacheConfiguration.Vary;
 
                        }
                        
                        cacheAdded = true;
                    }
                }

                if (cacheAdded == false && _options.ResponseCacheConfiguration != null && _options.ResponseCacheConfiguration.MaxAge != default)
                {
                    response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue() { Public = true, MaxAge = _options.ResponseCacheConfiguration.MaxAge };

                    if (_options.ResponseCacheConfiguration.Vary?.Any() == true)
                    {
                        response.Headers[HeaderNames.Vary] = _options.ResponseCacheConfiguration.Vary;
                    }
                    
                    cacheAdded = true;
                }

                // TODO: Log the actual used cache type
                // Log the actual cache values
                // Cache the cache info
                if (!cacheAdded)
                {
                    _logger.LogDebug("No cache configuration found for {Endpoint}", endpoint.Route);
                }
                else
                {
                    _logger.LogDebug("Cache configuration found for {Endpoint}. {CacheControl}, {Vary}.", endpoint.Route, response.GetTypedHeaders().CacheControl.ToString(), response.Headers[HeaderNames.Vary]);
                }
            }

            await _nextMiddleware(context);
        }
    }
}
