using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Extensions
{
    public class ApiFrameworkBuilder : IApiFrameworkBuilder
    {
        public ApiFrameworkBuilder(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
