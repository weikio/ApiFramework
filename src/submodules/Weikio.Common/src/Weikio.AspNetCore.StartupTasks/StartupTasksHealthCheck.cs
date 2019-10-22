using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.AspNetCore.StartupTasks
{
    public class StartupTasksHealthCheck : IHealthCheck
    {
        private readonly StartupTaskContext _context;
        private readonly StartupTasksHealthCheckParameters _parameters;

        public StartupTasksHealthCheck(StartupTaskContext context, StartupTasksHealthCheckParameters parameters)
        {
            _context = context;
            _parameters = parameters;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_context.IsComplete)
            {
                return Task.FromResult(HealthCheckResult.Healthy(_parameters.HealthyMessage));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy(_parameters.UnhealthyMessage));
        }
    }

    public class StartupTasksHealthCheckParameters
    {
        public bool IsEnabled { get; set; } = true;
        public string HealthyMessage { get; set; } = "All startup tasks complete";
        public string UnhealthyMessage { get; set; } = "Startup tasks not complete";
        public string HealthCheckName { get; set; } = "Startup tasks";
    }
}
