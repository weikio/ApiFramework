using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public interface IApiHealthCheckWrapper
    {
        Func<Endpoint, Task<IHealthCheck>> Wrap(MethodInfo healthCheckFactoryMethod);
    }
}
