using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

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
            Functions = await _apiProvider.List();

            return Page();
        }

        public List<ApiDefinition> Functions { get; set; }

        public async Task<ActionResult> OnPost()
        {
            var firstFunctionDef = (await _apiProvider.List()).First();
            var firstFunction = await _apiProvider.Get(firstFunctionDef);

            var newEndpoint = new Endpoint("/test", firstFunction, null);
            await newEndpoint.Initialize();

            EndpointManager.AddEndpoint(newEndpoint);
            EndpointManager.Update();

            return RedirectToPage();
        }
    }
}