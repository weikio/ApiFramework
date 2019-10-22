using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.HealthChecks;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class AppConfigurationEndpointConfigurationProvider : IEndpointConfigurationProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IApiProvider _apiProvider;

        public AppConfigurationEndpointConfigurationProvider(IConfiguration configuration, IApiProvider apiProvider)
        {
            _configuration = configuration;
            _apiProvider = apiProvider;
        }

        public async Task<List<EndpointConfiguration>> GetEndpointConfiguration()
        {
            var result = new List<EndpointConfiguration>();
            var functionFrameworkSection = _configuration.GetSection("ApiFramework");
            var hasConfig = functionFrameworkSection?.GetChildren()?.Any() == true;

            if (hasConfig == false)
            {
                return result;
            }

            var endpointsSection = functionFrameworkSection?.GetSection("Endpoints");

            if (endpointsSection == null)
            {
                return result;
            }

            foreach (var endpointSection in endpointsSection.GetChildren())
            {
                var definition = new ApiDefinition(endpointSection.GetValue<string>("Api"), new Version(1, 0, 0, 0));

                var route = endpointSection.Key;

                var functionConfigSection = endpointSection.GetSection("Configuration");
                var functionConfiguration = await GetApiConfiguration(functionConfigSection, definition);

                var endpointConfiguration = new EndpointConfiguration(route, definition.Name, functionConfiguration, new EmptyHealthCheck());

                result.Add(endpointConfiguration);
            }

            return result;
        }

        private async Task<Dictionary<Type, object>> GetApiConfiguration(IConfigurationSection rootConfigSection, ApiDefinition definition)
        {
            return null;

//            var result = new Dictionary<Type, object>();
//
//            if (rootConfigSection == null)
//            {
//                return result;
//            }
//
//            foreach (var configSection in rootConfigSection.GetChildren())
//            {
//                var function = await _functionProvider.Get(definition);
//
//                if (function == null)
//                {
//                    continue;
//                }
//
//                var configType = function.ConfigurationTypes
//                    .OrderBy(t => t.FullName)
//                    .FirstOrDefault(t => t.Name == configSection.Key);
//
//                if (configType == null)
//                {
//                    continue;
//                }
//
//                var config = configSection.Get(configType);
//                result.Add(configType, config);
//            }
//
//            return result;
        }
    }
}
