using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Samples.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly EndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;

        public IndexModel(EndpointManager endpointManager, IApiProvider apiProvider)
        {
            _endpointManager = endpointManager;
            _apiProvider = apiProvider;
        }

        public EndpointManager EndpointManager
        {
            get { return _endpointManager; }
        }

        public async Task<ActionResult> OnGet()
        {
            Apis = await _apiProvider.List();

            return Page();
        }

        public List<ApiDefinition> Apis { get; set; }

        public async Task<ActionResult> OnPost()
        {
            var firstApiDef = (await _apiProvider.List()).First();
            var firstApi = await _apiProvider.Get(firstApiDef);

            var newEndpoint = new Endpoint("/test", firstApi, null);
            await newEndpoint.Initialize();

            EndpointManager.AddEndpoint(newEndpoint);
            EndpointManager.Update();

            return RedirectToPage();
        }
    }
}
