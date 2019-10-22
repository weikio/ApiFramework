using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core.HealthChecks
{
    public class EndpointHeathCheck : IHealthCheck
    {
        private readonly StatusProvider _statusProvider;
        private readonly HealthProbe _healthProbe;

        public EndpointHeathCheck(StatusProvider statusProvider, HealthProbe healthProbe)
        {
            _statusProvider = statusProvider;
            _healthProbe = healthProbe;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            // This updates the health status for running & unhealthy endpoints
            await _healthProbe.ProbeEndpointsForHealth(context, cancellationToken);

            var status = await _statusProvider.Get();

            var endpointData = new Dictionary<string, object>();

            foreach (var endpoint in status.Endpoints)
            {
                endpointData.Add(endpoint.Route, endpoint.Status);
            }

            if (status.EndpointManagerStatusEnum == EndpointManagerStatusEnum.Running)
            {
                return HealthCheckResult.Healthy("Everything OK", endpointData);
            }

            if (status.EndpointManagerStatusEnum == EndpointManagerStatusEnum.Failed)
            {
                return HealthCheckResult.Unhealthy("Not OK", data: endpointData);
            }

            return HealthCheckResult.Degraded("Not fully functional", data: endpointData);
        }
    }
}
