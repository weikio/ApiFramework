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
        private readonly IFunctionProvider _functionProvider;

        public IndexModel(EndpointManager endpointManager, IFunctionProvider functionProvider)
        {
            _endpointManager = endpointManager;
            _functionProvider = functionProvider;
        }

        public EndpointManager EndpointManager
        {
            get { return _endpointManager; }
        }

        public async Task<ActionResult> OnGet()
        {
            Functions = await _functionProvider.List();

            return Page();
        }

        public List<FunctionDefinition> Functions { get; set; }

        public async Task<ActionResult> OnPost()
        {
            var firstFunctionDef = (await _functionProvider.List()).First();
            var firstFunction = await _functionProvider.Get(firstFunctionDef);

            var newEndpoint = new Endpoint("/test", firstFunction, null);
            await newEndpoint.Initialize();

            EndpointManager.AddEndpoint(newEndpoint);
            EndpointManager.Update();

            return RedirectToPage();
        }
    }
}