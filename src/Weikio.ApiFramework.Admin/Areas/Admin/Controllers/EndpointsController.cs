using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    [ApiController]
    [Route("/admin/api/endpoints")]
    [ApiExplorerSettings(GroupName = "api_framework_admin")]
    [ResponseCache(NoStore = true)]
    public class EndpointsController : ControllerBase
    {
        private readonly EndpointCollection _endpoints;
        private readonly EndpointInitializer _endpointInitializer;
        private readonly IApiProvider _apiProvider;
        private readonly EndpointManager _endpointManager;

        public EndpointsController(EndpointCollection endpoints, EndpointInitializer endpointInitializer, IApiProvider apiProvider,
            EndpointManager endpointManager)
        {
            _endpoints = endpoints;
            _endpointInitializer = endpointInitializer;
            _apiProvider = apiProvider;
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
            var apiDefinition = new ApiDefinition(endpointDto.Api.Name, Version.Parse(endpointDto.Api.Version));
            var api = await _apiProvider.Get(apiDefinition);

//            var configuration = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(endpointDto.JsonConfiguration);
//            
            // if (endpointDto.JsonConfiguration != null)
            // {
            //     foreach (var o in endpointDto.JsonConfiguration)
            //     {
            //         var key = o.Key;
            //         var val = ((JsonElement)o.Value).
            //     }
            // }
            var endpoint = new Endpoint(endpointDto.Route, api, endpointDto.JsonConfiguration);

            _endpointManager.AddEndpoint(endpoint);

            return Ok();
        }
    }
}
