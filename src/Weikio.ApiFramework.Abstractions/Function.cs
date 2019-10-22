using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Weikio.ApiFramework.Abstractions
{
    public class Function
    {
        public FunctionDefinition FunctionDefinition { get; }
        public List<Type> FunctionTypes { get; }
        public Func<Endpoint, Task<IEnumerable<Type>>> Initializer { get; }
        public Func<Endpoint, Task<IHealthCheck>> HealthCheckFactory { get; }

        public Function(FunctionDefinition functionDefinition, List<Type> functionTypes, Func<Endpoint, Task<IEnumerable<Type>>> initializer = null,
            Func<Endpoint, Task<IHealthCheck>> healthCheckFactory = null)
        {
            FunctionDefinition = functionDefinition;
            FunctionTypes = functionTypes;
            Initializer = initializer;
            HealthCheckFactory = healthCheckFactory;
        }
    }
}
