using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weikio.ApiFramework.Core;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    [ApiController]
    [Route("/admin/api/status")]
    [ApiExplorerSettings(GroupName = "api_framework_admin")]
    [ResponseCache(NoStore = true)]
    public class StatusController : ControllerBase
    {
        private readonly StatusProvider _statusProvider;

        public StatusController(StatusProvider statusProvider)
        {
            _statusProvider = statusProvider;
        }

        public async Task<ActionResult<StatusDto>> Get()
        {
            var status = await _statusProvider.Get();

            var result = new StatusDto
            {
                SystemStatus = status.EndpointManagerStatusEnum, AvailableApis = status.AvailableApis, Endpoints = new List<EndpointDto>()
            };

            foreach (var endpoint in status.Endpoints)
            {
                var dto = new EndpointDto(endpoint);
                result.Endpoints.Add(dto);
            }

            return result;
        }
    }
}
