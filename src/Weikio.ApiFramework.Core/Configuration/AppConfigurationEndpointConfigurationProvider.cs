﻿using System;
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
                var endpointGroupName = GetEndpointGroupName(endpointSection);

                var endpointDefinition = new EndpointDefinition(route, definition.Name, endpointConfiguration, new EmptyHealthCheck(), endpointGroupName);

                result.Add(endpointDefinition);
            }

            return Task.FromResult(result);
        }

        private string GetEndpointGroupName(IConfigurationSection endpointSection)
        {
            if (endpointSection == null)
            {
                return null;
            }

            var result = endpointSection.GetValue<string>("Group");

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
