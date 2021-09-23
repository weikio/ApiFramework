using System;
using System.Collections.Generic;
using System.Reflection;
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
        public Assembly Assembly { get; set; }

        public Api(ApiDefinition apiDefinition, List<Type> apiTypes, Assembly assembly, Func<Endpoint, Task<IEnumerable<Type>>> factory = null,
            Func<Endpoint, Task<IHealthCheck>> healthCheckFactory = null)
        {
            ApiDefinition = apiDefinition;
            ApiTypes = apiTypes;
            Assembly = assembly;
            Factory = factory;
            HealthCheckFactory = healthCheckFactory;
        }
    }

    public class ApiConfiguration
    {
        public ApiDefinition ApiDefinition { get; set; }
        public Type ConfigurationType { get; set; }

        public ApiConfiguration(ApiDefinition apiDefinition, Type configurationType)
        {
            ApiDefinition = apiDefinition;
            ConfigurationType = configurationType;
        }
    }
}
