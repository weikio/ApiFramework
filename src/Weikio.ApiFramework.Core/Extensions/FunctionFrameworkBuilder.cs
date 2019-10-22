using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Extensions
{
    public class FunctionFrameworkBuilder : IFunctionFrameworkBuilder
    {
        public FunctionFrameworkBuilder(IServiceCollection services)
        {
            Services = services;

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
        }

        public IServiceCollection Services { get; }
    }
}
