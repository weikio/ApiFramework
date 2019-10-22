using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.AspNetCore
{
    public class FunctionFrameworkAspNetCoreOptions
    {
        public bool AutoResolveFunctions { get; set; } = true;
        public string FunctionAddressBase { get; set; } = "/functions";
        public bool AutoResolveEndpoints { get; set; } = true;

        public List<(string Route, string FunctionAssemblyName, object Configuration, IHealthCheck healthCheck)> Endpoints { get; set; } =
            new List<(string Route, string FunctionAssemblyName, object Configuration, IHealthCheck healthCheck)>();
    }
}
