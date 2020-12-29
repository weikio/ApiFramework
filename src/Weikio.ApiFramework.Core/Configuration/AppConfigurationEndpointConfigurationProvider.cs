using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class AppConfigurationEndpointConfigurationProvider : IEndpointConfigurationProvider
    {
        private readonly IConfiguration _configuration;

        public AppConfigurationEndpointConfigurationProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<List<EndpointDefinition>> GetEndpointConfiguration()
        {
            var result = new List<EndpointDefinition>();
            var apiFrameworkConfigurationSection = _configuration.GetSection("ApiFramework");
            var hasConfig = apiFrameworkConfigurationSection?.GetChildren()?.Any() == true;

            if (hasConfig == false)
            {
                return Task.FromResult(result);
            }

            var endpointsSection = apiFrameworkConfigurationSection?.GetSection("Endpoints");

            if (endpointsSection == null)
            {
                return Task.FromResult(result);
            }

            foreach (var endpointSection in endpointsSection.GetChildren())
            {
                var definition = new ApiDefinition(endpointSection.GetValue<string>("Api"), new Version(1, 0, 0, 0));

                var route = endpointSection.Key;

                var endpointConfigurationSection = endpointSection.GetSection("Configuration");
                var endpointConfiguration = GetApiConfiguration(endpointConfigurationSection);
                var endpointGroupName = endpointSection.GetValue<string>("Group");
                var name = endpointSection.GetValue<string>("Name");
                var description = endpointSection.GetValue<string>("Description");
                var tags = endpointSection.GetSection("Tags")?.Get<string[]>();

                var endpointDefinition = new EndpointDefinition(route, definition.Name, endpointConfiguration, new EmptyHealthCheck(), endpointGroupName);
                endpointDefinition.Name = name;
                endpointDefinition.Description = description;
                endpointDefinition.Tags = tags;
                
                result.Add(endpointDefinition);
            }

            return Task.FromResult(result);
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
