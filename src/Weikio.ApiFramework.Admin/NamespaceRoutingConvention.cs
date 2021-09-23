using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Weikio.ApiFramework.Admin.Areas.Admin.Controllers;

namespace Weikio.ApiFramework.Admin
{
    public class NamespaceRoutingConvention : IControllerModelConvention
    {
        private readonly string _routeRoot;

        public NamespaceRoutingConvention(string routeRoot)
        {
            _routeRoot = routeRoot;
        }
        
        public void Apply(ControllerModel controller)
        {
            var hasRouteAttributes = controller.Selectors.Any(selector =>
                selector.AttributeRouteModel != null);

            if (!hasRouteAttributes)
            {
                return;
            }
            
            if (controller.ControllerType != typeof(EndpointsController))
            {
                return;
            }

            var template = $"{_routeRoot.Trim().Trim('/')}/[controller]";

            var routeModel = controller.Selectors.First(selector =>
                selector.AttributeRouteModel != null);

            var endpointRoute = routeModel.EndpointMetadata?.FirstOrDefault(x => x is RouteAttribute);

            if (endpointRoute != null)
            {
                routeModel.EndpointMetadata.Remove(endpointRoute);
                routeModel.EndpointMetadata.Add(new RouteAttribute(template));
            }

            routeModel.AttributeRouteModel.Template = template;
        }
    }
}
