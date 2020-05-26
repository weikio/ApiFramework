using System.Text.Json;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    public class EndpointDto
    {
        public EndpointDto(Endpoint endpoint)
        {
            Route = endpoint.Route;
            Api = endpoint.Api.ApiDefinition;
            Configuration = endpoint.Configuration == null ? null : JsonSerializer.Serialize(endpoint.Configuration);
            EndpointStatus = endpoint.Status;
        }

        public string Route { get; }
        public ApiDefinition Api { get; }
        public string Configuration { get; }
        public EndpointStatus EndpointStatus { get; set; }
    }
}
