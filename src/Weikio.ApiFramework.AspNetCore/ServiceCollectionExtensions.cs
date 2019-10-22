using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.FunctionProviders.PluginFramework;
using Weikio.PluginFramework;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.ApiFramework.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IFunctionFrameworkBuilder AddFunctionFramework(this IServiceCollection services, IMvcBuilder mvcBuilder,
            Action<FunctionFrameworkAspNetCoreOptions> setupAction = null)
        {
            var builder = services.AddFunctionFrameworkCore(mvcBuilder);
            builder.AddPluginFramework();

            if (setupAction != null)
            {
                var functionFrameworkAspnetCoreOptions = new FunctionFrameworkAspNetCoreOptions();
                setupAction(functionFrameworkAspnetCoreOptions);

                var setupFunctionFramework = new Action<FunctionFrameworkOptions>(options =>
                {
                    options.FunctionAddressBase = functionFrameworkAspnetCoreOptions.FunctionAddressBase;
                    options.AutoResolveEndpoints = functionFrameworkAspnetCoreOptions.AutoResolveEndpoints;
                    options.Endpoints = functionFrameworkAspnetCoreOptions.Endpoints;
                });

                builder.Services.Configure(setupFunctionFramework);

                var setupPluginFramework = new Action<PluginFrameworkFunctionProviderOptions>(options =>
                {
                    options.AutoResolveFunctions = functionFrameworkAspnetCoreOptions.AutoResolveFunctions;
                });

                builder.Services.Configure(setupPluginFramework);
            }

            return builder;
        }

        public static IFunctionFrameworkBuilder AddEndpoint(this IFunctionFrameworkBuilder builder, string route, string function, object configuration = null,
            IHealthCheck healthCheck = null)
        {
            builder.Services.AddTransient(services =>
            {
                var endpointConfiguration = new EndpointConfiguration(route, function, configuration, healthCheck);

                return endpointConfiguration;
            });

            return builder;
        }

        public static IFunctionFrameworkBuilder AddFunction(this IFunctionFrameworkBuilder builder, string assemblyPath)
        {
            builder.Services.AddTransient<IPluginCatalog>(services =>
            {
                var assemblyCatalog = new AssemblyPluginCatalog(assemblyPath);

                return assemblyCatalog;
            });

            return builder;
        }

        public static IFunctionFrameworkBuilder AddFunction(this IFunctionFrameworkBuilder builder, Assembly assembly)
        {
            builder.Services.AddTransient<IPluginCatalog>(services =>
            {
                var assemblyPath = assembly.Location;

                var assemblyCatalog = new AssemblyPluginCatalog(assemblyPath);

                return assemblyCatalog;
            });

            return builder;
        }

        public static IFunctionFrameworkBuilder AddFunction(this IFunctionFrameworkBuilder builder, Type functionType)
        {
            builder.Services.AddTransient<IPluginCatalog>(services =>
            {
                var assemblyCatalog = new TypePluginCatalog(functionType);

                return assemblyCatalog;
            });

            return builder;
        }
    }
}
