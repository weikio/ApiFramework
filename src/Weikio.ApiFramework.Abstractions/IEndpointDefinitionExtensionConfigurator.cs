using Microsoft.Extensions.Configuration;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointDefinitionExtensionConfigurator
    {
        void Configure(EndpointDefinition definition, IConfigurationSection configurationSection);
    }
}