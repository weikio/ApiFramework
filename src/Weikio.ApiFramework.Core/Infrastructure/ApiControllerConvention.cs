using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ApiControllerConvention : IControllerModelConvention
    {
        private readonly IEndpointManager _endpointManager;
        private readonly IEndpointRouteTemplateProvider _endpointRouteTemplateProvider;
        private readonly ILogger<ApiControllerConvention> _logger;
        private readonly ApiFrameworkOptions _options;

        public ApiControllerConvention(IEndpointManager endpointManager, IOptions<ApiFrameworkOptions> options,
            IEndpointRouteTemplateProvider endpointRouteTemplateProvider, ILogger<ApiControllerConvention> logger)
        {
            _endpointManager = endpointManager;
            _endpointRouteTemplateProvider = endpointRouteTemplateProvider;
            _logger = logger;
            _options = options.Value;
        }

        public void Apply(ControllerModel controller)
        {
            var endpoints = _endpointManager.Endpoints;

            List<(Type ControllerType, List<Endpoint> Endpoints)> apiControllers = endpoints
                .SelectMany(p => p.ApiTypes.Select(t => new { Endpoint = p, ControllerType = t }))
                .GroupBy(c => c.ControllerType)
                .Select(g => (g.Key, g.Select(c => c.Endpoint).ToList()))
                .ToList();

            foreach (var apiController in apiControllers)
            {
                if (!controller.ControllerType.IsAssignableFrom(apiController.ControllerType))
                {
                    continue;
                }

                if (controller.Selectors.Any())
                {
                    controller.Selectors.Clear();
                }

                var controllerName = apiController.ControllerType.Name;

                // Todo: Provide interface for configuring the controller naming
                if (controllerName.EndsWith("Api"))
                {
                    controllerName = controllerName.Substring(0, controllerName.Length - "Api".Length);
                }

                var controllerNameParts = controllerName.Split('_');
                controller.ControllerName = controllerNameParts.Last();

                var hasDuplicateEndpoints = apiController.Endpoints.GroupBy(x => x.Route).Any(x => x.Count() > 1);

                if (hasDuplicateEndpoints)
                {
                    _logger.LogWarning("Api {ApiName} has duplicate routes in endpoints. Adding index number to duplicate routes. Endpoint routes:",
                        apiController.ControllerType.FullName);

                    foreach (var endpoint in apiController.Endpoints)
                    {
                        _logger.LogWarning(endpoint.Route);
                    }
                }

                var routeCounts = new Dictionary<string, int>();

                foreach (var endpoint in apiController.Endpoints)
                {
                    var template = _endpointRouteTemplateProvider.GetRouteTemplate(endpoint);

                    if (controllerNameParts.Length > 1)
                    {
                        template = $"{template}/{string.Join("/", controllerNameParts.Take(controllerNameParts.Length - 1))}";
                    }

                    if (endpoint.ApiTypes.Count() > 1)
                    {
                        template = $"{template}/{controllerNameParts.Last()}";
                    }

                    if (routeCounts.ContainsKey(template) == false)
                    {
                        routeCounts[template] = 0;
                    }

                    routeCounts[template] += 1;

                    var currentCount = routeCounts[template];

                    if (currentCount > 1)
                    {
                        template = template + (currentCount - 1);
                    }

                    var item = new SelectorModel { AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(template)) };

                    // Endpoint metadata seems to be bugged as it always returns the same instance even though the route changes.
                    // This issue seems to be fixed in .NET Core 3

                    item.EndpointMetadata.Add(endpoint);
                    var endpointConfiguration = endpoint.Configuration;

                    if (endpointConfiguration != null)
                    {
                        // Primary option is to try to bind to Configuration-property, if such exists
                        // It should be ok to use reflection here. The convention is only run when something changes.
                        var configProperty = controller.ControllerType.GetProperties().FirstOrDefault(p => p.Name == "Configuration");

                        if (configProperty != null)
                        {
                            // Create configuration setter delegate. This is executed from <see cref="ApiConfigurationActionFilter"/>
                            // var convertedConfigValue = JsonSerializer.Deserialize(JsonSerializer.Serialize(endpointConfiguration), configProperty.PropertyType);

                            var config = endpointConfiguration;

                            if (endpointConfiguration.GetType() != configProperty.PropertyType)
                            {
                                config = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(endpointConfiguration), configProperty.PropertyType);
                            }

                            item.EndpointMetadata.Add(config);

                            item.EndpointMetadata.Add(new Action<object>(obj =>
                            {
                                configProperty.SetValue(obj, config);
                            }));
                        }
                        else if (endpoint.Configuration is IDictionary<string, object> dictionary && dictionary.Count > 1)
                        {
                            // If we have configuration for the endpoint but no Configuration-property in the Api, go through the configuration's keys one by one
                            var controllerProperties = controller.ControllerType.GetProperties().ToList();
                            var setters = new List<Action<object>>();

                            foreach (var keyValue in dictionary)
                            {
                                var prop = controllerProperties.FirstOrDefault(x =>
                                    string.Equals(x.Name, keyValue.Key, StringComparison.InvariantCultureIgnoreCase));

                                if (prop == null)
                                {
                                    continue;
                                }

                                var convertedConfigValue = JsonSerializer.Deserialize(JsonSerializer.Serialize(keyValue.Value), prop.PropertyType);
                                item.EndpointMetadata.Add(convertedConfigValue);

                                setters.Add(obj =>
                                {
                                    prop.SetValue(obj, convertedConfigValue);
                                });
                            }

                            if (setters.Any())
                            {
                                item.EndpointMetadata.Add(new Action<object>(obj =>
                                {
                                    foreach (var setter in setters)
                                    {
                                        setter.Invoke(obj);
                                    }
                                }));
                            }
                        }
                        else
                        {
                            // We don't know what to do with the configuration.
                            // TODO: Throw?
                        }
                    }

                    if (endpoint?.Metadata?.Any() == true)
                    {
                        foreach (var o in endpoint.Metadata)
                        {
                            item.EndpointMetadata.Add(o);
                        }
                    }

                    controller.Selectors.Add(item);
                }

                controller.ApiExplorer = new ApiExplorerModel { IsVisible = true, GroupName = "api_framework_endpoint" };
            }
        }
    }
}
