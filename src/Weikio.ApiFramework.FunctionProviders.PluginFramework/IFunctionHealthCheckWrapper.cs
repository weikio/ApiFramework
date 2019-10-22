using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.FunctionProviders.PluginFramework
{
    public interface IFunctionHealthCheckWrapper
    {
        Func<Endpoint, Task<IHealthCheck>> Wrap(MethodInfo healthCheckFactoryMethod);
    }
}
