using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
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
        private readonly IServiceProvider _serviceProvider;

        public ApiController(IApiProvider apiProvider, IServiceProvider serviceProvider)
        {
            _apiProvider = apiProvider;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ApiDefinition>> GetApis()
        {    
            var result = _apiProvider.List();

            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult> RegisterNuget(string packageName, string version, string feedUrl)
        {
            var catalog = NugetPackageFactory.CreateApiCatalog(packageName, version, _serviceProvider, feedUrl);
            await catalog.Initialize(CancellationToken.None);
            
            _apiProvider.Add(catalog);

            return Ok();
        }
    }
}
