﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ApiActionConvention : IActionModelConvention
    {
        private readonly IEndpointManager _endpointManager;
        private readonly IEndpointHttpVerbResolver _endpointHttpVerbResolver;
        private readonly IOptions<ApiFrameworkOptions> _optionsAccessor;
        private readonly IOptionsMonitor<AutoTidyUrlAPIOverrides> _autoTidyUrlOverridesAccessor;

        public ApiActionConvention(IEndpointManager endpointManager, IEndpointHttpVerbResolver endpointHttpVerbResolver,
            IOptions<ApiFrameworkOptions> optionsAccessor, IOptionsMonitor<AutoTidyUrlAPIOverrides> autoTidyUrlOverridesAccessor)
        {
            _endpointManager = endpointManager;
            _endpointHttpVerbResolver = endpointHttpVerbResolver;
            _optionsAccessor = optionsAccessor;
            _autoTidyUrlOverridesAccessor = autoTidyUrlOverridesAccessor;
        }

        public void Apply(ActionModel action)
        {
            var endpoints = _endpointManager.Endpoints;

            var endpointsAndTypes = endpoints
                .SelectMany(p => p.ApiTypes.Select(t => new { Endpoint = p, ControllerType = t }))
                .ToList();

            foreach (var endpointAndTypes in endpointsAndTypes)
            {
                if (!action.Controller.ControllerType.IsAssignableFrom(endpointAndTypes.ControllerType))
                {
                    continue;
                }

                var hasFixedHttpConventions = action.Attributes.FirstOrDefault(x => x.GetType() == typeof(FixedHttpConventionsAttribute)) != null;

                if (hasFixedHttpConventions)
                {
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

                HandleUrlTidying(action, httpVerb, selector, endpointAndTypes.Endpoint);

                if (action.Selectors.Any())
                {
                    action.Selectors.Clear();
                }

                action.Selectors.Add(selector);
            }
        }

        private void HandleUrlTidying(ActionModel action, string httpVerb, SelectorModel selector, Endpoint endpoint)
        {
            // By default Api Framework removes the method name from endpoint if there is only one method in the Api with the same HTTP Method.
            // Example: HelloWorld.SayHello is available from [Get]HelloWorld instead of [Get]HelloWorld/SayHello
            // If there is multiple Get (or Post, or Delete..) methods in same Api, method name is automatically added to avoid ambiguous routes.
            // ApiFrameworkOptions.AutoTidyUrls can be used to control this functionality

            var autoTidyUrls = GetAutoTidyUrlMode(endpoint, _optionsAccessor, _autoTidyUrlOverridesAccessor);

            // Can cause ambiguous routes
            if (autoTidyUrls == AutoTidyUrlModeEnum.Always)
            {
                return;
            }

            if (autoTidyUrls == AutoTidyUrlModeEnum.Disabled)
            {
                selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(action.ActionName));

                return;
            }

            var hasMultipleActions = action.Controller.Actions.Count > 1;

            if (hasMultipleActions)
            {
                var httpVerbs = action.Controller.Actions.Select(x => _endpointHttpVerbResolver.GetHttpVerb(x))
                    .GroupBy(x => x).ToList();

                var hasMultipleSameHttpMethods = httpVerbs.FirstOrDefault(x => string.Equals(x.Key, httpVerb))?.Count() > 1;

                if (hasMultipleSameHttpMethods)
                {
                    selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(action.ActionName));
                }
            }
        }

        internal static AutoTidyUrlModeEnum GetAutoTidyUrlMode(Endpoint endpoint, IOptions<ApiFrameworkOptions> optionsAccessor, IOptionsMonitor<AutoTidyUrlAPIOverrides> autoTidyUrlOverridesAccessor)
        {
            var endpointOptions = optionsAccessor.Value;
            var autoTidyUrls = endpointOptions.AutoTidyUrls;

            // First try to check if we have API specific override for the auto tidy. If not, use the global default.
            // Endpoint specific AutoTidy configuration is not yet possible, see #24
            var apiName = endpoint.Api.ApiDefinition.Name;
            var apiOverride = autoTidyUrlOverridesAccessor.Get(apiName);

            if (apiOverride.IsSet)
            {
                autoTidyUrls = apiOverride.AutoTidyUrls;
            }

            return autoTidyUrls;
        }
    }
}
