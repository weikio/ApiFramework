using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class CodeBasedEndpointConfigurationProvider : IEndpointConfigurationProvider
    {
        private readonly List<EndpointConfiguration> _configurations = new List<EndpointConfiguration>();

        public CodeBasedEndpointConfigurationProvider()
        {
        }

        public CodeBasedEndpointConfigurationProvider Add(EndpointConfiguration configuration)
        {
            _configurations.Add(configuration);

            return this;
        }

        public Task<List<EndpointConfiguration>> GetEndpointConfiguration()
        {
            return Task.FromResult(_configurations);
        }
    }
}
