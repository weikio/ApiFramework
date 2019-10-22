using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointConfigurationProvider
    {
        Task<List<EndpointConfiguration>> GetEndpointConfiguration();
    }
}
