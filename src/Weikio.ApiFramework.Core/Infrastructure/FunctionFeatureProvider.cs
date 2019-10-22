﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class FunctionFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly EndpointManager _endpointManager;

        public FunctionFeatureProvider(EndpointManager endpointManager)
        {
            _endpointManager = endpointManager;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var controllerTypes = _endpointManager.Endpoints
                .SelectMany(p => p.FunctionTypes)
                .ToArray();

            foreach (var controllerType in controllerTypes)
            {
                var existing = feature.Controllers.FirstOrDefault(x => x.AsType() == controllerType);

                if (existing != null)
                {
                    continue;
                }

                feature.Controllers.Add(controllerType.GetTypeInfo());
            }
        }
    }
}
