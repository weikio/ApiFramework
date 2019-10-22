using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Abstractions
{
    public class EndpointConfiguration
    {
        public EndpointConfiguration(string route, string api, object configuration, IHealthCheck healthCheck)
        {
            Route = route;
            Api = api;
            Configuration = configuration;
            HealthCheck = healthCheck;
        }

        public string Route { get; }
        public string Api { get; }
        public object Configuration { get; }
        public IHealthCheck HealthCheck { get; }
    }
}
