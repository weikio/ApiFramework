using System.Collections.Generic;

namespace Weikio.ApiFramework.Extensions.ResponceCache
{
    public class ResponceCacheOptions
    {
        public ResponseCacheConfiguration ResponseCacheConfiguration { get; set; }

        public Dictionary<string, EndpointResponceCacheConfiguration> EndpointResponceCacheConfigurations { get; set; } =
            new Dictionary<string, EndpointResponceCacheConfiguration>();
    }
}