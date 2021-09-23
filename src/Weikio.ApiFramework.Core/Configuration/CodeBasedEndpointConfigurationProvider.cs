using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class CodeBasedEndpointConfigurationProvider : IEndpointConfigurationProvider
    {
        private readonly List<Func<EndpointDefinition>> _configurations = new List<Func<EndpointDefinition>>();

        public CodeBasedEndpointConfigurationProvider()
        {
        }

        public CodeBasedEndpointConfigurationProvider Add(EndpointDefinition definition)
        {
            _configurations.Add(() => definition);

            return this;
        }

        public CodeBasedEndpointConfigurationProvider Add(Func<EndpointDefinition> definitionFactory)
        {
            _configurations.Add(definitionFactory);

            return this;
        }

        public Task<List<EndpointDefinition>> GetEndpointConfiguration()
        {
            var result = new List<EndpointDefinition>();

            foreach (var configuration in _configurations)
            {
                var def = configuration();
                result.Add(def);
            }
            
            return Task.FromResult(result);
        }
    }
}
