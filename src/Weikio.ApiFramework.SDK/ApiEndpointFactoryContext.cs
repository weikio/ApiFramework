using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.SDK
{
    public class ApiEndpointFactoryContext
    {
        public Endpoint Endpoint { get; set; }
        public object AssemblyLoadContext { get; set; }
    }

    public class ApiFactoryResult
    {
        public List<Type> Types { get; set; }
        public IServiceCollection Services { get; set; }

        public ApiFactoryResult(Type type, IServiceCollection services = null) : this(new List<Type>() { type }, services) { }

        public ApiFactoryResult(List<Type> types, IServiceCollection services = null)
        {
            Types = types;
            Services = services;
        }
    }
}
