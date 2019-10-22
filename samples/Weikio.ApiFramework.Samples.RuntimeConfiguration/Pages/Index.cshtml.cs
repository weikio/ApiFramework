using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

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
            Functions = await _apiProvider.List();

            return Page();
        }

        public List<ApiDefinition> Functions { get; set; }

        public async Task<ActionResult> OnPost()
        {
            var function = await _apiProvider.Get("Weikio.ApiFramework.Plugins.HelloWorld");
            var newEndpoint = new Endpoint("/test", function, null);
            newEndpoint.Initialize();

            EndpointManager.AddEndpoint(newEndpoint);
            EndpointManager.Update();

            return RedirectToPage();
        }
    }
}