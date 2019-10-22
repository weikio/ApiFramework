using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    [ApiController]
    [Route("/admin/api/endpoints")]
    [ApiExplorerSettings(GroupName = "function_framework_admin")]
    public class EndpointsController : ControllerBase
    {
        private readonly EndpointCollection _endpoints;
        private readonly EndpointInitializer _endpointInitializer;
        private readonly IFunctionProvider _functionProvider;
        private readonly EndpointManager _endpointManager;

        public EndpointsController(EndpointCollection endpoints, EndpointInitializer endpointInitializer, IFunctionProvider functionProvider,
            EndpointManager endpointManager)
        {
            _endpoints = endpoints;
            _endpointInitializer = endpointInitializer;
            _functionProvider = functionProvider;
            _endpointManager = endpointManager;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EndpointDto>> GetEndpoints()
        {
            return Ok(_endpoints.Select(x => new EndpointDto(x)));
        }

        [HttpPost("{route}/initialize")]
        public async Task<ActionResult> Initialize(string route)
        {
            route = HttpUtility.UrlDecode(route);

            var endpoint = _endpoints.FirstOrDefault(x => string.Equals(x.Route, route, StringComparison.InvariantCultureIgnoreCase));

            if (endpoint == null)
            {
                return NotFound($"Endpoint not found with {route}");
            }

            await _endpointInitializer.Initialize(endpoint, true);

            return Ok();
        }

        [HttpPost("")]
        public async Task<ActionResult> Add([FromBody] NewEndpointDto endpointDto)
        {
            var functionDefinition = new FunctionDefinition(endpointDto.Function.Name, Version.Parse(endpointDto.Function.Version));
            var function = await _functionProvider.Get(functionDefinition);

//            var configuration = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(endpointDto.JsonConfiguration);
//            
            var endpoint = new Endpoint(endpointDto.Route, function, endpointDto.JsonConfiguration);

            _endpointManager.AddEndpoint(endpoint);

            return Ok();
        }
    }
}
