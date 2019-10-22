using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class FunctionControllerConvention : IControllerModelConvention
    {
        private readonly EndpointManager _endpointManager;
        private readonly FunctionFrameworkOptions _options;

        public FunctionControllerConvention(EndpointManager endpointManager, IOptions<FunctionFrameworkOptions> options)
        {
            _endpointManager = endpointManager;
            _options = options.Value;
        }

        public void Apply(ControllerModel controller)
        {
            var endpoints = _endpointManager.Endpoints;

            List<(Type ControllerType, List<Endpoint> Endpoints)> functionControllers = endpoints
                .SelectMany(p => p.FunctionTypes.Select(t => new { Endpoint = p, ControllerType = t }))
                .GroupBy(c => c.ControllerType)
                .Select(g => (g.Key, g.Select(c => c.Endpoint).ToList()))
                .ToList();

            foreach (var functionController in functionControllers)
            {
                if (!controller.ControllerType.IsAssignableFrom(functionController.ControllerType))
                {
                    continue;
                }

                if (controller.Selectors.Any())
                {
                    controller.Selectors.Clear();
                }

                var controllerName = functionController.ControllerType.Name;

                // Todo: Provide interface for configuring the controller naming
                if (controllerName.EndsWith("Function"))
                {
                    controllerName = controllerName.Substring(0, controllerName.Length - "Function".Length);
                }

                var controllerNameParts = controllerName.Split('_');
                controller.ControllerName = controllerNameParts.Last();

                foreach (var endpoint in functionController.Endpoints)
                {
                    var template = GetRouteTemplate(endpoint.Route);

                    if (controllerNameParts.Length > 1)
                    {
                        template = $"{template}/{string.Join("/", controllerNameParts.Take(controllerNameParts.Length - 1))}";
                    }

                    if (endpoint.FunctionTypes.Count() > 1)
                    {
                        template = $"{template}/{controllerNameParts.Last()}";
                    }

                    var item = new SelectorModel { AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(template)) };

                    // Endpoint metadata seems to be bugged as it always returns the same instance even though the route changes.
                    // This issue seems to be fixed in .NET Core 3

                    item.EndpointMetadata.Add(endpoint);
                    var endpointConfiguration = endpoint.Configuration;

                    if (endpointConfiguration != null)
                    {
                        // It should be ok to use reflection here. The convention is only run when something changes.
                        var configProperty = controller.ControllerType.GetProperties().FirstOrDefault(p => p.Name == "Configuration");

                        if (configProperty != null)
                        {
                            // Create configuration setter delegate. This is executed from <see cref="FunctionConfigurationActionFilter"/>
                            var convertedConfigValue =
                                JsonSerializer.Deserialize(JsonSerializer.Serialize(endpointConfiguration),
                                    configProperty.PropertyType);
                            item.EndpointMetadata.Add(convertedConfigValue);

                            item.EndpointMetadata.Add(new Action<object>(obj =>
                            {
                                configProperty.SetValue(obj, convertedConfigValue);
                            }));
                        }
                    }

                    controller.Selectors.Add(item);
                }

                controller.ApiExplorer = new ApiExplorerModel { IsVisible = true, GroupName = "function_framework_function" };
            }
        }

        private string GetRouteTemplate(string endpointRoute)
        {
            if (string.IsNullOrWhiteSpace(endpointRoute))
            {
                throw new ArgumentNullException(nameof(endpointRoute));
            }

            if (string.IsNullOrWhiteSpace(_options.FunctionAddressBase))
            {
                return endpointRoute;
            }

            if (_options.FunctionAddressBase.EndsWith('/'))
            {
                return _options.FunctionAddressBase + endpointRoute.TrimStart('/').TrimEnd('/').Trim();
            }

            return _options.FunctionAddressBase + '/' + endpointRoute.TrimStart('/').TrimEnd('/').Trim();
        }
    }
}
