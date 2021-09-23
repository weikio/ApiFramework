using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core
{
    public class HealthProbe
    {
        private readonly IEndpointManager _endpointManager;
        private readonly ILogger<HealthProbe> _logger;

        public HealthProbe(IEndpointManager endpointManager, ILogger<HealthProbe> logger)
        {
            _endpointManager = endpointManager;
            _logger = logger;
        }

        public async Task ProbeEndpointsForHealth(HealthCheckContext context, CancellationToken token)
        {
            var endpoints = _endpointManager.Endpoints;

            if (endpoints?.Any() != true)
            {
                return;
            }

            var endpointsWithHealthCheck = endpoints.Where(x => x.HasHealthCheck).ToList();

            foreach (var endpoint in endpointsWithHealthCheck)
            {
                try
                {
                    if (endpoint.Status.Status == EndpointStatusEnum.Ready)
                    {
                        var endpointHealthTask = endpoint.HealthCheck;
                        var endpointHealthCheckResult = await endpointHealthTask.CheckHealthAsync(context, token);

                        if (endpointHealthCheckResult.Status == HealthStatus.Healthy)
                        {
                            continue;
                        }

                        endpoint.Status.UpdateStatus(EndpointStatusEnum.Unhealthy,
                            $"Health check returned {endpointHealthCheckResult.Status}: {endpointHealthCheckResult.Description}");

                        continue;
                    }

                    if (endpoint.Status.Status == EndpointStatusEnum.Unhealthy)
                    {
                        var endpointHealthTask = endpoint.HealthCheck;
                        var endpointHealthCheckResult = await endpointHealthTask.CheckHealthAsync(context, token);

                        if (endpointHealthCheckResult.Status != HealthStatus.Healthy)
                        {
                            continue;
                        }

                        endpoint.Status.UpdateStatus(EndpointStatusEnum.Ready,
                            $"Health check returned {endpointHealthCheckResult.Status}: {endpointHealthCheckResult.Description}");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Health check for {Endpoint} threw exception. Health checks shouldn't throw, instead they should return Unhealthy.");

                    if (endpoint.Status.Status == EndpointStatusEnum.Ready)
                    {
                        endpoint.Status.UpdateStatus(EndpointStatusEnum.Unhealthy, $"Health check threw error: {e}");
                    }
                }
            }
        }
    }
}
