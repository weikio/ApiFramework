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
        private readonly IEndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;

        public IndexModel(IEndpointManager endpointManager, IApiProvider apiProvider)
        {
            _endpointManager = endpointManager;
            _apiProvider = apiProvider;
        }

        public IEndpointManager EndpointManager
        {
            get { return _endpointManager; }
        }

        public ActionResult OnGet()
        {
            Apis = _apiProvider.List();

            return Page();
        }

        public List<ApiDefinition> Apis { get; set; }

        public async Task<ActionResult> OnPost()
        {
            var api = _apiProvider.Get("Weikio.ApiFramework.Plugins.HelloWorld");
            var newEndpoint = _endpointManager.Create("/test", api, null);
            await newEndpoint.Initialize();

            EndpointManager.AddEndpoint(newEndpoint);
            EndpointManager.Update();

            return RedirectToPage();
        }
    }
}
