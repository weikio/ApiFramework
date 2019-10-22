using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core.Caching
{
    public class ResponseCacheControlMiddleware
    {
        private readonly Dictionary<PathString, ResponseCacheConfiguration> _cacheConfigs = new Dictionary<PathString, ResponseCacheConfiguration>();
        private readonly RequestDelegate _nextMiddleware;

        public ResponseCacheControlMiddleware(EndpointCollection endpoints, RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware;

            foreach (var endpoint in endpoints)
            {
                foreach (var cacheConfig in endpoint.ResponseCacheConfigurations)
                {
                    var path = new PathString("/" + endpoint.Route.Trim('/'));

                    var resourcePath = cacheConfig.Value.Path.Trim('/');

                    if (resourcePath != "")
                    {
                        path = path.Add("/" + resourcePath);
                    }

                    _cacheConfigs.Add(path, cacheConfig.Value);
                }
            }
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var requestPathParts = new Queue<string>(request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries));

            while (requestPathParts.Any())
            {
                var path = new PathString("/" + string.Join('/', requestPathParts));

                if (_cacheConfigs.TryGetValue(path, out var cacheConfig))
                {
                    response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue() { Public = true, MaxAge = cacheConfig.MaxAge };

                    if (cacheConfig.Vary.Any())
                    {
                        response.Headers[HeaderNames.Vary] = cacheConfig.Vary;
                    }

                    break;
                }

                requestPathParts.Dequeue();
            }

            await _nextMiddleware(context);
        }
    }
}
