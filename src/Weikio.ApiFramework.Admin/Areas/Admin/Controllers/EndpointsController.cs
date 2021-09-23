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
    [ApiExplorerSettings(GroupName = "api_framework_admin")]
    [ResponseCache(NoStore = true)]
    public class EndpointsController : ControllerBase
    {
        private readonly EndpointInitializer _endpointInitializer;
        private readonly IApiProvider _apiProvider;
        private readonly IEndpointManager _endpointManager;

        public EndpointsController(EndpointInitializer endpointInitializer, IApiProvider apiProvider,
            IEndpointManager endpointManager)
        {
            _endpointInitializer = endpointInitializer;
            _apiProvider = apiProvider;
            _endpointManager = endpointManager;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EndpointDto>> GetEndpoints()
        {
            return Ok(_endpointManager.Endpoints.Select(x => new EndpointDto(x)));
        }

        [HttpPost("{route}/initialize")]
        public async Task<ActionResult> Initialize(string route)
        {
            route = HttpUtility.UrlDecode(route);

            var endpoint = _endpointManager.Endpoints.FirstOrDefault(x => string.Equals(x.Route, route, StringComparison.InvariantCultureIgnoreCase));

            if (endpoint == null)
            {
                return NotFound($"Endpoint not found with {route}");
            }

            await _endpointInitializer.Initialize(endpoint, true);

            return Ok();
        }

        [HttpPost("")]
        public ActionResult Add([FromBody] NewEndpointDto endpointDto)
        {
            var apiDefinition = new ApiDefinition(endpointDto.Api.Name, Version.Parse(endpointDto.Api.Version));
            var api = _apiProvider.Get(apiDefinition);

            _endpointManager.CreateAndAdd(endpointDto.Route, api, endpointDto.JsonConfiguration);

            return Ok();
        }
        
        [HttpDelete("")]
        public ActionResult Remove(string route)
        {
            var endpoint = _endpointManager.Endpoints.FirstOrDefault(x => string.Equals(x.Route, route, StringComparison.InvariantCultureIgnoreCase));

            if (endpoint == null)
            {
                return NotFound($"Endpoint not found with {route}");
            }
            
            _endpointManager.RemoveEndpoint(endpoint);

            return Ok();
        }
    }
}
