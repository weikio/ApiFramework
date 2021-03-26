using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.SDK
{
    public interface IEndpointConfigurationProvider
    {
        Task<List<EndpointDefinition>> GetEndpointConfiguration();
    }
}
