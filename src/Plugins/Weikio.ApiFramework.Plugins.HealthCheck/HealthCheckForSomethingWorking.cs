using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Plugins.HealthCheck
{
    public class HealthCheckForSomethingWorking : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var isBroken = SometimesWorkingApi.IsBroken();

            if (isBroken)
            {
                var errorResult = new HealthCheckResult(HealthStatus.Unhealthy);

                return Task.FromResult(errorResult);
            }

            var okResult = new HealthCheckResult(HealthStatus.Healthy);

            return Task.FromResult(okResult);
        }
    }
}
