using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Abstractions
{
    public class EndpointDefinition
    {
        public EndpointDefinition(string route, string api, object configuration, IHealthCheck healthCheck, string groupName)
        {
            Route = route;
            Api = api;
            Configuration = configuration;
            HealthCheck = healthCheck;
            GroupName = groupName;
        }

        public string Route { get; }
        public string Api { get; }
        public object Configuration { get; }
        public IHealthCheck HealthCheck { get; }
        public string GroupName { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
    }
}
