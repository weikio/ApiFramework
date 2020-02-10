using System;
using System.Collections.Generic;

namespace Weikio.ApiFramework.Abstractions
{
    public class EndpointResponceCacheConfiguration
    {
        public EndpointResponceCacheConfiguration(TimeSpan defaultMaxAge, string[] defaultVary)
        {
            DefaultMaxAge = defaultMaxAge;
            DefaultVary = defaultVary;
            PathConfigurations = new Dictionary<string, ResponseCacheConfiguration>();
        }

        public void AddPathConfiguration(string path, ResponseCacheConfiguration configuration)
        {
            //TODO: Check for duplicates
            PathConfigurations.Add(path, configuration);
        }

        public TimeSpan DefaultMaxAge { get; }
        public string[] DefaultVary { get; }

        public Dictionary<string, ResponseCacheConfiguration> PathConfigurations { get; set; }
    }
    
    public class ResponseCacheConfiguration
    {
        // public string Path { get; }

        public TimeSpan MaxAge { get; }

        public string[] Vary { get; }

        public ResponseCacheConfiguration(TimeSpan maxAge, string[] vary)
        {
            MaxAge = maxAge;
            Vary = vary;
        }
    }
}
