using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Core.Configuration;

namespace Weikio.ApiFramework.AspNetCore
{
    public class ApiFrameworkAspNetCoreOptions
    {
        public bool AutoResolveApis { get; set; } = false;
        public string ApiAddressBase { get; set; } = "/api";
        public bool AutoResolveEndpoints { get; set; } = false;

        public List<(string Route, string ApiAssemblyName, object Configuration, IHealthCheck healthCheck, string GroupName)> Endpoints { get; set; } =
            new List<(string Route, string ApiAssemblyName, object Configuration, IHealthCheck healthCheck, string groupName)>();
        
        public bool AutoInitializeApiProvider { get; set; } = true;
        public bool AutoInitializeConfiguredEndpoints { get; set; } = true;
        public bool AutoConvertFileToStream { get; set; } = true;
        public bool AddHealthCheck { get; set; } = true;
        
        /// <summary>
        /// Gets or sets if urls should be automatically tidied. Default = Automatic
        /// </summary>
        public AutoTidyUrlModeEnum AutoTidyUrls { get; set; } = AutoTidyUrlModeEnum.Automatic;
    }
}
