using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.TypeFinding;

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
                services.AddHealthChecks()
                    .AddCheck<EndpointHeathCheck>("api_framework_endpoint", HealthStatus.Degraded, new[] { "api_framework_endpoint" });

                return builder;
            }

            var apiFrameworkAspNetCoreOptions = new ApiFrameworkAspNetCoreOptions();
            setupAction(apiFrameworkAspNetCoreOptions);

            if (apiFrameworkAspNetCoreOptions.AddHealthCheck)
            {
                services.AddHealthChecks()
                    .AddCheck<EndpointHeathCheck>("api_framework_endpoint", HealthStatus.Degraded, new[] { "api_framework_endpoint" });
            }

            var setupApiFramework = new Action<ApiFrameworkOptions>(options =>
            {
                options.ApiAddressBase = apiFrameworkAspNetCoreOptions.ApiAddressBase;
                options.AutoResolveEndpoints = apiFrameworkAspNetCoreOptions.AutoResolveEndpoints;
                options.Endpoints = apiFrameworkAspNetCoreOptions.Endpoints;
                options.AutoInitializeApiProvider = apiFrameworkAspNetCoreOptions.AutoInitializeApiProvider;
                options.AutoInitializeConfiguredEndpoints = apiFrameworkAspNetCoreOptions.AutoInitializeConfiguredEndpoints;
                options.AutoConvertFileToStream = apiFrameworkAspNetCoreOptions.AutoConvertFileToStream;
                options.AutoTidyUrls = apiFrameworkAspNetCoreOptions.AutoTidyUrls;
                options.ApiCatalogs = apiFrameworkAspNetCoreOptions.ApiCatalogs;
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


        public static IApiFrameworkBuilder AddApi<T>(this IApiFrameworkBuilder builder, string route = null, object configuration = null,
            IHealthCheck healthCheck = null, string groupName = null)
        {
            return builder.AddApi(typeof(T), route, configuration, healthCheck, groupName);
        }
        
        
        public static IApiFrameworkBuilder AddApi(this IApiFrameworkBuilder builder, Type apiType, string route = null, object configuration = null,
            IHealthCheck healthCheck = null, string groupName = null)
        {
            builder.Services.AddTransient<IPluginCatalog>(services =>
            {
                var options = services.GetRequiredService<IOptions<PluginFrameworkApiProviderOptions>>().Value;
                var criteria = options.CreateTypeApiFinderCriteria(apiType);
                
                var catalogOptions = new TypePluginCatalogOptions()
                {
                    TypeFinderOptions = new TypeFinderOptions() { TypeFinderCriterias = criteria }
                };
                
                var typeCatalog = new TypePluginCatalog(apiType, catalogOptions);

                return typeCatalog;
            });

            if (string.IsNullOrWhiteSpace(route))
            {
                return builder;
            }

            builder.Services.AddTransient(services =>
            {
                var endpointConfiguration = new EndpointDefinition(route, apiType.FullName, configuration, endpoint => Task.FromResult(healthCheck), groupName);

                return endpointConfiguration;
            });

            return builder;
        }
    }
}
