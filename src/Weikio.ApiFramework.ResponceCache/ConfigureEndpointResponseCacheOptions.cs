using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Weikio.ApiFramework.ResponceCache
{
    public class ConfigureEndpointResponseCacheOptions : IConfigureOptions<ResponceCacheOptions>
    {
        private readonly IConfiguration _configuration;

        public ConfigureEndpointResponseCacheOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(ResponceCacheOptions options)
        {
            var apiFrameworkConfigurationSection = _configuration.GetSection("ApiFramework");
            var hasConfig = apiFrameworkConfigurationSection?.GetChildren()?.Any() == true;

            if (hasConfig == false)
            {
                return;
            }

            var cacheSection = apiFrameworkConfigurationSection.GetSection("Cache");

            if (cacheSection?.GetChildren()?.Any() == true)
            {
                var age = cacheSection.GetValue<TimeSpan>("MaxAge");
                var vary = cacheSection.GetSection("Vary")?.Get<string[]>() ?? new string[] { };

                options.ResponseCacheConfiguration = new ResponseCacheConfiguration(age, vary);
            }
            else
            {
                options.ResponseCacheConfiguration = new ResponseCacheConfiguration(default, Array.Empty<string>());
            }

            var endpointsSection = apiFrameworkConfigurationSection?.GetSection("Endpoints");

            if (endpointsSection == null)
            {
                return;
            }

            foreach (var endpointSection in endpointsSection.GetChildren())
            {
                var endpointCacheSection = endpointSection.GetSection("Cache");

                if (endpointCacheSection?.GetChildren()?.Any() != true)
                {
                    continue;
                }

                var endpointRoute = endpointSection.Key;
                var defaultAge = endpointCacheSection.GetValue<TimeSpan>("MaxAge");
                var vary = endpointCacheSection.GetSection("Vary")?.Get<string[]>() ?? new string[] { };

                var endpointResponceCacheConfiguration = new EndpointResponceCacheConfiguration(endpointRoute)
                {
                    ResponseCacheConfiguration = new ResponseCacheConfiguration(defaultAge, vary)
                };

                var configurations = GetResponseCacheConfigurations(endpointCacheSection.GetSection("routes"));

                foreach (var responseCacheConfiguration in configurations)
                {
                    endpointResponceCacheConfiguration.AddPathConfiguration(responseCacheConfiguration.Key, responseCacheConfiguration.Value);
                }

                if (options.EndpointResponceCacheConfigurations == null)
                {
                    options.EndpointResponceCacheConfigurations = new Dictionary<string, EndpointResponceCacheConfiguration>();
                }

                options.EndpointResponceCacheConfigurations.Add(endpointRoute, endpointResponceCacheConfiguration);
            }
        }

        private static Dictionary<string, ResponseCacheConfiguration> GetResponseCacheConfigurations(IConfigurationSection cachingConfigSection)
        {
            var cacheConfigs = new Dictionary<string, ResponseCacheConfiguration>(StringComparer.OrdinalIgnoreCase);

            if (cachingConfigSection?.GetChildren()?.Any() != true)
            {
                return cacheConfigs;
            }

            foreach (var configSection in cachingConfigSection.GetChildren())
            {
                var path = configSection.GetValue<string>("Route");
                var maxAge = configSection.GetValue<TimeSpan>("MaxAge");
                var vary = configSection.GetSection("Vary")?.Get<string[]>() ?? new string[] { };

                cacheConfigs.Add(path, new ResponseCacheConfiguration(maxAge, vary));
            }

            return cacheConfigs;
        }
    }
}