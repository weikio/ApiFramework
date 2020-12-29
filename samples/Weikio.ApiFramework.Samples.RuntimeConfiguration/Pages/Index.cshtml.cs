using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.SDK;

namespace RuntimeConfiguration.Pages
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
            Apis = _apiProvider.List();

            return Page();
        }

        public List<ApiDefinition> Apis { get; set; }

        public async Task<ActionResult> OnPost()
        {
            var api = _apiProvider.Get("Weikio.ApiFramework.Plugins.HelloWorld");
            var newEndpoint = new Endpoint("/test", api, null);
            await newEndpoint.Initialize();

            EndpointManager.AddEndpoint(newEndpoint);
            EndpointManager.Update();

            return RedirectToPage();
        }
    }
}
