using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
                var functionConfiguration = await GetApiConfiguration(endpointSection, definition, _configuration);

                var endpointConfiguration = new EndpointConfiguration(route, definition.Name, functionConfiguration, new EmptyHealthCheck());

                result.Add(endpointConfiguration);
            }

            return result;
        }

        public class SourceSettings
        {
            public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>(); 
        }
        
        private async Task<Dictionary<string, object>> GetApiConfiguration(IConfigurationSection rootConfigSection, ApiDefinition definition,
            IConfiguration configuration)
        {
            var result = new SourceSettings();

            if (rootConfigSection == null)
            {
                return null;
            }



            try
            {
                var damn = rootConfigSection.GetValue<string>("Configuration");
                var rgr = _configuration.GetValue<string>("ApiFramework");
                
                var settings = _configuration
                                                         .GetSection("ApiFramework")
                                                         .Get<System.Text.Json.JsonElement>();
//                string json = System.Text.Json.JsonSerializer.Serialize(settings);
                
//                var duh = rootConfigSection.Get<string>();
//
//
//                var configJson = _configuration.Get<string>(options => options.BindNonPublicProperties = true);
//                rootConfigSection.Bind(result);
//                var values = rootConfigSection
//                    .GetChildren()
//                    .ToList();
//
//                foreach (var value in values)
//                {
//                
//                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }

            return null;

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
