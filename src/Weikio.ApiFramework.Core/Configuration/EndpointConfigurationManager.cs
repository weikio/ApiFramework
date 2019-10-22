using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class EndpointConfigurationManager
    {
        private readonly IEnumerable<IEndpointConfigurationProvider> _configurationProviders;

        public EndpointConfigurationManager(IEnumerable<IEndpointConfigurationProvider> configurationProviders)
        {
            _configurationProviders = configurationProviders;
        }

        public async Task<List<EndpointConfiguration>> GetEndpointConfigurations()
        {
            var result = new List<EndpointConfiguration>();

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
