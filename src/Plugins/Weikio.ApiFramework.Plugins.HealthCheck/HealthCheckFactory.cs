using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Plugins.HealthCheck
{
    public static class HealthCheckFactory
    {
        public static Task<IHealthCheck> Create()
        {
            var result = new HealthCheckForSomethingWorking();

            return Task.FromResult<IHealthCheck>(result);
        }
    }
}
