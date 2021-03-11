using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Infrastructure;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class ApiFrameworkOptions
    {
        public bool RequireConfiguration { get; set; } = false;
        public bool UseConfiguration { get; set; } = true;
        public string ApiAddressBase { get; set; } = "/api";

        public IEndpointHttpVerbResolver EndpointHttpVerbResolver { get; set; }

//        public Func<MetadataReader, TypeDefinition, bool> FunctionResolver { get; set; } = FunctionLocator.IsFunction;
        public bool AutoResolveEndpoints { get; set; } = false;

//        public bool AutoResolveFunctions { get; set; } = true;
//        public List<string> FunctionAssemblies { get; set; } = new List<string>();
        public List<(string Route, string ApiAssemblyName, object Configuration, IHealthCheck HealthCheck, string GroupName)> Endpoints { get; set; } =
            new List<(string Route, string ApiAssemblyName, object Configuration, IHealthCheck HealthCheck, string groupName)>();

        public List<IApiCatalog> ApiCatalogs { get; set; } = new List<IApiCatalog>();

        /// <summary>
        /// Gets or sets how endpoint changes are notified to the system. 
        /// </summary>
        public ChangeNotificationTypeEnum ChangeNotificationType { get; set; } = ChangeNotificationTypeEnum.Single;

        public bool AutoInitializeApiProvider { get; set; } = true;
        public bool AutoInitializeConfiguredEndpoints { get; set; } = true;
        public bool AutoConvertFileToStream { get; set; } = true;
        
        /// <summary>
        /// Gets or sets if urls should be automatically tidied. Default = Automatic
        /// </summary>
        public AutoTidyUrlModeEnum AutoTidyUrls { get; set; } = AutoTidyUrlModeEnum.Automatic;
    }
}
