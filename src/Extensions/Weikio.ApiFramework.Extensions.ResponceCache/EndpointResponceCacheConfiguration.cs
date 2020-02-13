using System.Collections.Generic;

namespace Weikio.ApiFramework.Extensions.ResponceCache
{
    public class EndpointResponceCacheConfiguration
    {
        public string EndpointRoute { get; set; }
        public ResponseCacheConfiguration ResponseCacheConfiguration { get; set; }

        public EndpointResponceCacheConfiguration(string endpointRoute)
        {
            EndpointRoute = endpointRoute;
            PathConfigurations = new Dictionary<string, ResponseCacheConfiguration>();
        }

        public void AddPathConfiguration(string path, ResponseCacheConfiguration configuration)
        {
            //TODO: Check for duplicates
            PathConfigurations.Add(path, configuration);
        }

        public Dictionary<string, ResponseCacheConfiguration> PathConfigurations { get; set; }
    }
}