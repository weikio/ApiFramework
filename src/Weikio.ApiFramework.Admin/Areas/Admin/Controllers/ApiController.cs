using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    [ApiController]
    [Route("/admin/api/apis")]
    [ApiExplorerSettings(GroupName = "api_framework_admin")]
    [ResponseCache(NoStore = true)]
    public class ApiController : ControllerBase
    {
        private readonly IApiProvider _apiProvider;

        public ApiController(IApiProvider apiProvider)
        {
            _apiProvider = apiProvider;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApiDefinition>>> GetApis()
        {    
            var result = await _apiProvider.List();

            return Ok(result);
        }
    }
}
