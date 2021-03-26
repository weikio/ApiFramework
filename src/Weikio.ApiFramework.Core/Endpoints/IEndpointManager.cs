using System.Collections.Generic;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Endpoints
{
    public interface IEndpointManager
    {
        EndpointManagerStatusEnum Status { get; }
        List<Endpoint> Endpoints { get; }
        Endpoint Create(EndpointDefinition endpointDefinition);
        Endpoint CreateAndAdd(EndpointDefinition endpointDefinition);
        void AddEndpoint(Endpoint endpoint);

        /// <summary>
        /// Updates the current runtime status to match the configuration. If new endpoints are added runtime, these are not applied automatically.
        /// </summary>
        void Update();

        void RemoveEndpoint(Endpoint endpoint);
    }
}
