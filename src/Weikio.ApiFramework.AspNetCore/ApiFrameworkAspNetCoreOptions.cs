using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.AspNetCore
{
    public class ApiFrameworkAspNetCoreOptions
    {
        public bool AutoResolveApis { get; set; } = true;
        public string ApiAddressBase { get; set; } = "/api";
        public bool AutoResolveEndpoints { get; set; } = true;

        public List<(string Route, string ApiAssemblyName, object Configuration, IHealthCheck healthCheck, string GroupName)> Endpoints { get; set; } =
            new List<(string Route, string ApiAssemblyName, object Configuration, IHealthCheck healthCheck, string groupName)>();
        
        public bool AutoInitializeApiProvider { get; set; } = true;
        public bool AutoInitializeConfiguredEndpoints { get; set; } = true;
    }
}
