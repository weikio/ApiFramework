using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class FunctionActionConvention : IActionModelConvention
    {
        private readonly EndpointManager _endpointManager;

        //private readonly Func<EndpointCollection> _endpointsFactory;

        public FunctionActionConvention(EndpointManager endpointManager)
        {
            _endpointManager = endpointManager;

            //_endpointsFactory = endpointsFactory;
        }

        public void Apply(ActionModel action)
        {
            var endpoints = _endpointManager.Endpoints;

            var dynamicControllersTypes = endpoints
                .SelectMany(p => p.FunctionTypes)
                .ToList();

            foreach (var dynamicControllerType in dynamicControllersTypes)
            {
                if (!action.Controller.ControllerType.IsAssignableFrom(dynamicControllerType))
                {
                    continue;
                }

                var useBody = false;

                var selector = new SelectorModel { };

                var httpVerb = FunctionHttpVerbResolver.GetHttpVerb(action);

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
                    var httpVerbs = action.Controller.Actions.Select(x => FunctionHttpVerbResolver.GetHttpVerb(x))
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

                action.Selectors.Add(selector);
            }
        }
    }
}
