using System.Text.Json;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    public class EndpointDto
    {
        public EndpointDto(Endpoint endpoint)
        {
            Route = endpoint.Route;
            Function = endpoint.Function.FunctionDefinition;
            Configuration = endpoint.Configuration == null ? null : JsonSerializer.Serialize(endpoint.Configuration);
            EndpointStatus = endpoint.Status;
        }

        public string Route { get; }
        public FunctionDefinition Function { get; }
        public string Configuration { get; }
        public EndpointStatus EndpointStatus { get; set; }
    }
}
