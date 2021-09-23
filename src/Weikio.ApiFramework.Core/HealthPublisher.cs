using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Core
{
    /// <summary>
    /// Class which makes sure that our endpoint health checks are automatically executed time to time
    /// </summary>
    public class HealthPublisher : IHealthCheckPublisher
    {
        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
