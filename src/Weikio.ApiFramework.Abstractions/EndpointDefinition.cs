using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Abstractions
{
    public class EndpointDefinition
    {
        public EndpointDefinition()
        {
        }

        public EndpointDefinition(string route, ApiDefinition api, object configuration = null, Func<Endpoint, Task<IHealthCheck>> healthCheckFactory = null, 
            string groupName = null, string name = null, string description = null, string[] tags = null, string policyName = "")
        {
            Route = route;
            Api = api;
            Configuration = configuration;
            HealthCheckFactory = healthCheckFactory;
            GroupName = groupName;
            Name = name;
            Description = description;
            Tags = tags;
            Policy = policyName;
        }

        public string Route { get; set; }
        public ApiDefinition Api { get; set; }
        public object Configuration { get; set; }
        public Func<Endpoint, Task<IHealthCheck>> HealthCheckFactory { get; set; }
        public string GroupName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public string Policy { get; set; }
    }
}
