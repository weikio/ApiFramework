using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ApiActionConvention : IActionModelConvention
    {
        private readonly EndpointManager _endpointManager;
        private readonly IEndpointHttpVerbResolver _endpointHttpVerbResolver;
        private readonly ODataOptions _oDataOptions;

        //private readonly Func<EndpointCollection> _endpointsFactory;

        public ApiActionConvention(EndpointManager endpointManager, IEndpointHttpVerbResolver endpointHttpVerbResolver, IOptions<ODataOptions> oDataOptions)
        {
            _endpointManager = endpointManager;
            _endpointHttpVerbResolver = endpointHttpVerbResolver;
            _oDataOptions = oDataOptions.Value;
        }

        public void Apply(ActionModel action)
        {
            var endpoints = _endpointManager.Endpoints;

            var dynamicControllersTypes = endpoints
                .SelectMany(p => p.ApiTypes)
                .ToList();

            foreach (var dynamicControllerType in dynamicControllersTypes)
            {
                if (!action.Controller.ControllerType.IsAssignableFrom(dynamicControllerType))
                {
                    continue;
                }

                var hasFixedHttpConventions = action.Attributes.FirstOrDefault(x => x.GetType() == typeof(FixedHttpConventionsAttribute)) != null;


                if (hasFixedHttpConventions)
                {
                    continue;
                }

                if (action.ActionMethod.Name == "Close" || action.ActionMethod.Name == "Abort" || action.ActionMethod.Name == "Open" || action.ActionMethod.Name == "OpenAsync" || action.ActionMethod.Name == "CloseAsync")
                {
                    action.Selectors.Clear();
                    action.ApiExplorer.IsVisible = false;

                    continue;
                }
                
                var useBody = false;

                var selector = new SelectorModel { };

                var httpVerb = _endpointHttpVerbResolver.GetHttpVerb(action);

                if (!string.Equals(httpVerb, "GET"))
                {
                    var constraint = new HttpMethodActionConstraint(new[] { httpVerb });
                    selector.ActionConstraints.Add(constraint);
                }

                if (string.Equals(httpVerb, "POST") || string.Equals(httpVerb, "PUT"))
                {
                    useBody = true;
                }

                foreach (var parameterModel in action.Parameters)
                {
                    if (parameterModel.BindingInfo?.BindingSource == null && !parameterModel.Attributes.OfType<IBindingSourceMetadata>().Any() &&
                        !parameterModel.ParameterInfo.ParameterType.CanBeConvertedFromString())
                    {
                        if (useBody)
                        {
                            parameterModel.BindingInfo = parameterModel.BindingInfo ?? new BindingInfo();
                            parameterModel.BindingInfo.BindingSource = BindingSource.Body;
                        }
                    }

                    if (parameterModel.ParameterType.IsInterface)
                    {
                        parameterModel.BindingInfo = new BindingInfo() { BindingSource = BindingSource.Services };
                    }
                }

                var hasMultipleActions = action.Controller.Actions.Count > 1;

                if (hasMultipleActions)
                {
                    var httpVerbs = action.Controller.Actions.Select(x => _endpointHttpVerbResolver.GetHttpVerb(x))
                        .GroupBy(x => x).ToList();

                    if (httpVerbs.FirstOrDefault(x => string.Equals(x.Key, httpVerb))?.Count() > 1)
                    {
                        selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(action.ActionName));
                    }
                }

                if (action.Selectors.Any())
                {
                    action.Selectors.Clear();
                }

                var model = _oDataOptions.Models["odata"].Item1;
                var entitySet = model.EntityContainer.FindEntitySet("Customer");
                
                var path = new ODataPathTemplate(new EntitySetSegmentTemplate(entitySet));
                var odataMetadata = new ODataRoutingMetadata("odata", model, path);

                // action.AddSelector("get", "odata", model, path);
                selector.EndpointMetadata.Add(odataMetadata);
                selector.EndpointMetadata.Add(new EndpointNameMetadata(Guid.NewGuid().ToString()));

                action.Selectors.Add(selector);
            }
        }
    }
}
