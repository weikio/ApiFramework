using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Infrastructure;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class FunctionFrameworkOptions
    {
        public bool RequireConfiguration { get; set; } = false;
        public bool UseConfiguration { get; set; } = true;
        public string FunctionAddressBase { get; set; } = "/functions";

        public Func<ActionModel, string> HttpVerbResolver { get; set; } = FunctionHttpVerbResolver.GetHttpVerb;

//        public Func<MetadataReader, TypeDefinition, bool> FunctionResolver { get; set; } = FunctionLocator.IsFunction;
        public bool AutoResolveEndpoints { get; set; } = true;

//        public bool AutoResolveFunctions { get; set; } = true;
//        public List<string> FunctionAssemblies { get; set; } = new List<string>();
        public List<(string Route, string FunctionAssemblyName, object Configuration, IHealthCheck HealthCheck)> Endpoints { get; set; } =
            new List<(string Route, string FunctionAssemblyName, object Configuration, IHealthCheck HealthCheck)>();

        public IFunctionProvider FunctionProvider { get; set; }

        /// <summary>
        /// Gets or sets how endpoint changes are notified to the system. 
        /// </summary>
        public ChangeNotificationTypeEnum ChangeNotificationType { get; set; } = ChangeNotificationTypeEnum.Single;
    }
}
