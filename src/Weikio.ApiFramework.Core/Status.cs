using System.Collections.Generic;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core
{
    public class Status
    {
        public EndpointManagerStatusEnum EndpointManagerStatusEnum { get; set; }
        public List<ApiDefinition> AvailableApis { get; set; }
        public List<Endpoint> Endpoints { get; set; }
    }
}
