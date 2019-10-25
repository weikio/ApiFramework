using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class CodeBasedEndpointConfigurationProvider : IEndpointConfigurationProvider
    {
        private readonly List<EndpointDefinition> _configurations = new List<EndpointDefinition>();

        public CodeBasedEndpointConfigurationProvider()
        {
        }

        public CodeBasedEndpointConfigurationProvider Add(EndpointDefinition definition)
        {
            _configurations.Add(definition);

            return this;
        }

        public Task<List<EndpointDefinition>> GetEndpointConfiguration()
        {
            return Task.FromResult(_configurations);
        }
    }
}
