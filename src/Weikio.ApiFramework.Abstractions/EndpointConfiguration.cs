using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Abstractions
{
    public class EndpointConfiguration
    {
        public EndpointConfiguration(string route, string function, object configuration, IHealthCheck healthCheck)
        {
            Route = route;
            Function = function;
            Configuration = configuration;
            HealthCheck = healthCheck;
        }

        public string Route { get; }
        public string Function { get; }
        public object Configuration { get; }
        public IHealthCheck HealthCheck { get; }
    }
}
