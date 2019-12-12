using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Endpoints
{
    public interface IEndpointInitializer
    {
        void Initialize(List<Endpoint> endpoints, bool force = false);
        Task Initialize(Endpoint endpoint, bool force = false);
    }
}
