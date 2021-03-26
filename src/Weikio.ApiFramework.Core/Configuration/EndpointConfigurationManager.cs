using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class EndpointConfigurationManager
    {
        private readonly IEnumerable<IEndpointConfigurationProvider> _configurationProviders;
        private readonly IApiByAssemblyProvider _apiByAssemblyProvider;

        public EndpointConfigurationManager(IEnumerable<IEndpointConfigurationProvider> configurationProviders, IApiByAssemblyProvider apiByAssemblyProvider)
        {
            _configurationProviders = configurationProviders;
            _apiByAssemblyProvider = apiByAssemblyProvider;
        }

        public async Task<List<EndpointDefinition>> GetEndpointDefinitions()
        {
            var result = new List<EndpointDefinition>();

            foreach (var endpointConfigurationProvider in _configurationProviders)
            {
                var endpointConfigurations = await endpointConfigurationProvider.GetEndpointConfiguration();

                // TODO: Check for duplicate routes 
                result.AddRange(endpointConfigurations);
            }

            return result;
        }
    }
}
