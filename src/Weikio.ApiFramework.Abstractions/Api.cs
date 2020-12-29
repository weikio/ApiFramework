using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Abstractions
{
    public class Api
    {
        public ApiDefinition ApiDefinition { get; }
        public List<Type> ApiTypes { get; }
        public Func<Endpoint, Task<IEnumerable<Type>>> Factory { get; }
        public Func<Endpoint, Task<IHealthCheck>> HealthCheckFactory { get; }

        public Api(ApiDefinition apiDefinition, List<Type> apiTypes, Func<Endpoint, Task<IEnumerable<Type>>> factory = null,
            Func<Endpoint, Task<IHealthCheck>> healthCheckFactory = null)
        {
            ApiDefinition = apiDefinition;
            ApiTypes = apiTypes;
            Factory = factory;
            HealthCheckFactory = healthCheckFactory;
        }
    }
}
