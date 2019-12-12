using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.ApiProviders.PluginFramework;

namespace ApiFramework.IntegrationTests
{
    public class ApiHealthCheckWrapperForUnitTests : IApiHealthCheckWrapper
    {
        public Func<Endpoint, Task<IHealthCheck>> Wrap(MethodInfo healthCheckFactoryMethod)
        {
            return null;
        }
    }
}
