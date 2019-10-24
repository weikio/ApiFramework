using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Dynamic;
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
                var functionConfiguration = GetApiConfiguration(functionConfigSection);

                var endpointConfiguration = new EndpointConfiguration(route, definition.Name, functionConfiguration, new EmptyHealthCheck());

                result.Add(endpointConfiguration);
            }

            return result;
        }

        private IDictionary<string, object> GetApiConfiguration(IConfigurationSection configuration)
        {
            if (configuration == null)
            {
                return null;
            }
            
            return configuration.ToDictionary();
        }
    }
}
