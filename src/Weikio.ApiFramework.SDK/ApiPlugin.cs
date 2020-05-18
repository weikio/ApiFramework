using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.SDK
{
    public class ApiPlugin
    {
        public Assembly Assembly { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string ProductVersion { get; set; }
        public List<Type> ApiTypes { get; set; } 
        public List<Type> FactoryTypes { get; set; } 
        public IHealthCheck HealthCheckType { get; set; } 
    }

    public class ApiPluginOptions
    {
        public List<Assembly> ApiPluginAssemblies { get; set; } = new List<Assembly>();
    }
}
