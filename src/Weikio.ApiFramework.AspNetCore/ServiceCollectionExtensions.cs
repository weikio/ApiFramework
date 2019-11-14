﻿using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.ApiFramework.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddApiFramework(this IMvcBuilder mvcBuilder, Action<ApiFrameworkAspNetCoreOptions> setupAction = null)
        {
            return mvcBuilder.Services.AddApiFramework(setupAction);
        }
        
        public static IApiFrameworkBuilder AddApiFramework(this IServiceCollection services,
            Action<ApiFrameworkAspNetCoreOptions> setupAction = null)
        {
            var builder = services.AddApiFrameworkCore();
            builder.AddPluginFramework();

            if (setupAction == null)
            {
                return builder;
            }

            var apiFrameworkAspNetCoreOptions = new ApiFrameworkAspNetCoreOptions();
            setupAction(apiFrameworkAspNetCoreOptions);

            var setupApiFramework = new Action<ApiFrameworkOptions>(options =>
            {
                options.ApiAddressBase = apiFrameworkAspNetCoreOptions.ApiAddressBase;
                options.AutoResolveEndpoints = apiFrameworkAspNetCoreOptions.AutoResolveEndpoints;
                options.Endpoints = apiFrameworkAspNetCoreOptions.Endpoints;
            });

            builder.Services.Configure(setupApiFramework);

            var setupPluginFramework = new Action<PluginFrameworkApiProviderOptions>(options =>
            {
                options.AutoResolveApis = apiFrameworkAspNetCoreOptions.AutoResolveApis;
            });

            builder.Services.Configure(setupPluginFramework);

            return builder;
        }

        public static IApiFrameworkBuilder AddApi(this IApiFrameworkBuilder builder, string assemblyPath)
        {
            builder.Services.AddTransient<IPluginCatalog>(services =>
            {
                var assemblyCatalog = new AssemblyPluginCatalog(assemblyPath);

                return assemblyCatalog;
            });

            return builder;
        }

        public static IApiFrameworkBuilder AddApi(this IApiFrameworkBuilder builder, Assembly assembly)
        {
            builder.Services.AddTransient<IPluginCatalog>(services =>
            {
                var assemblyPath = assembly.Location;

                var assemblyCatalog = new AssemblyPluginCatalog(assemblyPath);

                return assemblyCatalog;
            });

            return builder;
        }

        public static IApiFrameworkBuilder AddApi(this IApiFrameworkBuilder builder, Type apiType)
        {
            builder.Services.AddTransient<IPluginCatalog>(services =>
            {
                var assemblyCatalog = new TypePluginCatalog(apiType);

                return assemblyCatalog;
            });

            return builder;
        }
    }
}
