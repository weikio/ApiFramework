using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Weikio.ApiFramework.ResponceCache
{
    public class ConfigureEndpointResponseCacheOptions : IConfigureOptions<ResponceCacheOptions>
    {
        private readonly ILogger<ConfigureEndpointResponseCacheOptions> _logger;
        private readonly IConfiguration _configuration;

        public ConfigureEndpointResponseCacheOptions(ILogger<ConfigureEndpointResponseCacheOptions> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void Configure(ResponceCacheOptions options)
        {
            _logger.LogInformation("Configuring endpoint response cache");

            var parsed = true;

            try
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

                    var configurations = GetResponseCacheConfigurations(endpointCacheSection.GetSection("Routes"));

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
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to parse endpoint response cache configuration");
                parsed = false;

                throw;
            }
            finally
            {
                if (parsed)
                {
                    LogConfiguration(options);
                }
            }
        }

        private void LogConfiguration(ResponceCacheOptions options)
        {
            _logger.LogInformation("Endpoint response cache configured");
            _logger.LogDebug($"Global cache: {options.ResponseCacheConfiguration}");

            if (options.EndpointResponceCacheConfigurations?.Any() == true)
            {
                var logMessage = new StringBuilder();
                logMessage.AppendLine("Endpoint response caches:");
                
                foreach (var endpointResponceCacheConfiguration in options.EndpointResponceCacheConfigurations)
                {
                    logMessage.AppendLine($"{endpointResponceCacheConfiguration.Key}: {endpointResponceCacheConfiguration.Value.ResponseCacheConfiguration}");

                    if (endpointResponceCacheConfiguration.Value.PathConfigurations?.Any() != true)
                    {
                        continue;
                    }

                    foreach (var endpointRouteResponceCacheConfiguration in endpointResponceCacheConfiguration.Value.PathConfigurations)
                    {
                        logMessage.AppendLine(
                            $"{endpointResponceCacheConfiguration.Key}{endpointRouteResponceCacheConfiguration.Key}: {endpointRouteResponceCacheConfiguration.Value}");
                    }
                }
                _logger.LogDebug(logMessage.ToString().Trim());
            }
            else
            {
                _logger.LogDebug("No endpoint specific response cache configurations set");
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
