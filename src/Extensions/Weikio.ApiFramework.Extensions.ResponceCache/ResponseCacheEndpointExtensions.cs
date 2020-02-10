using System;
using System.Linq;

namespace Weikio.ApiFramework.Abstractions
{
    public static class ResponseCacheEndpointExtensions
    {
        public static ResponseCacheConfiguration GetResponseCacheConfiguration(this Endpoint endpoint)
        {
            var extension = endpoint.Extensions.FirstOrDefault(x => string.Equals(x.Key, "RESPONSE_CACHE", StringComparison.InvariantCultureIgnoreCase));

            return extension?.Data as ResponseCacheConfiguration;
        }
    }
}