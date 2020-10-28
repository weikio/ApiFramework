using System.Collections.Generic;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    public class StatusDto
    {
        public List<EndpointDto> Endpoints { get; set; }

        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public EndpointManagerStatusEnum SystemStatus { get; set; }

        public List<ApiDefinition> AvailableApis { get; set; }
    }
}
