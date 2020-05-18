using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.SDK
{
    public class PluginEndpointConfigurationProvider : IEndpointConfigurationProvider
    {
        private readonly EndpointDefinition _endpointDefinition;

        public PluginEndpointConfigurationProvider(EndpointDefinition endpointDefinition)
        {
            _endpointDefinition = endpointDefinition;
        }

        public Task<List<EndpointDefinition>> GetEndpointConfiguration()
        {
            return Task.FromResult(new List<EndpointDefinition>() { _endpointDefinition });
        }
    }
}
