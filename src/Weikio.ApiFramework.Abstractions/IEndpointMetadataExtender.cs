using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.ApiFramework.Abstractions
{
    public interface IEndpointMetadataExtender
    {
        Task<List<object>> GetMetadata(Endpoint endpoint);
    }
}
