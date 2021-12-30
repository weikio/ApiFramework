using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.ApiFramework.Core;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.AsyncStream;
using Weikio.ApiFramework.Core.Cache;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.ApiFramework.Core.StartupTasks;
using Weikio.ApiFramework.SDK;
using Weikio.AspNetCore.Common;
using Weikio.AspNetCore.StartupTasks;

// ReSharper disable once CheckNamespace
namespace Weikio.ApiFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddApiFrameworkCore(this IServiceCollection services,
            Action<ApiFrameworkOptions> setupAction = null)
        {
            var builder = new ApiFrameworkBuilder(services);

            services.AddSingleton<IApiCatalog>(provider =>
            {
                var configurationOptions = provider.GetService<IOptions<ApiFrameworkOptions>>();
                var options = configurationOptions != null ? configurationOptions.Value : new ApiFrameworkOptions();

                if (options.ApiCatalogs?.Any() != true)
                {
                    return new EmptyApiCatalog();
                }

                var compositeCatalog = new CompositeApiCatalog();

                foreach (var apiCatalog in options.ApiCatalogs)
                {
                    compositeCatalog.Add(apiCatalog);
                }

                return compositeCatalog;
            });

            services.TryAddSingleton(provider =>
            {
                var configurationOptions = provider.GetService<IOptions<ApiFrameworkOptions>>();
                var options = configurationOptions != null ? configurationOptions.Value : new ApiFrameworkOptions();

                if (options.EndpointHttpVerbResolver != null)
                {
                    return options.EndpointHttpVerbResolver;
                }

                return new DefaultEndpointHttpVerbResolver();
            });

            services.TryAddSingleton<IEndpointStartupHandler, EndpointStartupHandler>();
            services.AddHostedService<ApiFrameworkStartupFilter>();

            services.TryAddSingleton<ApiChangeNotifier>();
            services.TryAddSingleton<EndpointConfigurationManager>();

            services.TryAddSingleton<IEndpointManager, DefaultEndpointManager>();

            services.TryAddSingleton<EndpointInitializer>();

            services.TryAddSingleton<ApiChangeToken>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IActionDescriptorChangeProvider, ActionDescriptorChangeProvider>());

            services.AddHttpContextAccessor();

            services.TryAddSingleton<ApiControllerConvention>();
            services.TryAddSingleton<ApiActionConvention>();
            services.TryAddSingleton<ApiFeatureProvider>();

            // Services for running background tasks like endpoint initializations
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.TryAddSingleton<StatusProvider>();
            services.TryAddSingleton<HealthProbe>();
            services.TryAddSingleton<IApiProviderInitializer, ApiProviderInitializer>();
            services.TryAddSingleton<IEndpointInitializer, EndpointInitializer>();
            services.TryAddSingleton<IEndpointRouteTemplateProvider, DefaultEndpointRouteTemplateProvider>();

            services.TryAddSingleton<IApiProvider, DefaultApiProvider>();

            // Services which alter the group names of the API Descriptions. These are used for Open Api / Swagger generation. Each endpoint by default belongs to an unique api group.
            services.TryAddSingleton<IEndpointGroupNameProvider, EndpointGroupNameProvider>();
            services.TryAddSingleton<IDefaultEndpointGroupNameProvider, DefaultEndpointGroupNameProvider>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, EndpointGroupNameDescriptor>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, ApiFileResponseTypeDescriptor>());

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHealthCheckPublisher, HealthPublisher>());

            builder.Services.TryAddTransient<IAsyncStreamJsonHelperFactory, DefaultAsyncStreamJsonHelperFactory>();
            builder.Services.AddTransient<SystemTextAsyncStreamJsonHelper>();
            builder.Services.AddTransient<NewtonsoftAsyncStreamJsonHelper>();
            builder.Services.AddTransient<AsyncJsonActionFilter>();
            builder.Services.TryAddSingleton<IApiByAssemblyProvider, DefaultApiByAssemblyProvider>();
            builder.Services.TryAddSingleton<IApiConfigurationTypeProvider, DefaultApiConfigurationTypeProvider>();

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.AddService<AsyncJsonActionFilter>();
            });

            services.TryAddSingleton<IEndpointConfigurationProvider>(provider =>
            {
                var result = new CodeBasedEndpointConfigurationProvider();
                var configurationOptions = provider.GetService<IOptions<ApiFrameworkOptions>>();
                ApiFrameworkOptions options = null;

                if (configurationOptions != null)
                {
                    options = configurationOptions.Value;
                }
                else
                {
                    options = new ApiFrameworkOptions();
                }

                foreach (var endpoint in options.Endpoints)
                {
                    var endpointConfiguration = new EndpointDefinition(endpoint.Route, endpoint.ApiAssemblyName,
                        endpoint.Configuration, endpoint1 => Task.FromResult(endpoint.HealthCheck), endpoint.GroupName);

                    result.Add(endpointConfiguration);
                }

                var registeredEndpoints = provider.GetServices(typeof(EndpointDefinition)).Cast<EndpointDefinition>().ToList();

                if (registeredEndpoints?.Any() == true)
                {
                    foreach (var endpointConfiguration in registeredEndpoints)
                    {
                        result.Add(endpointConfiguration);
                    }
                }

                var registeredEndpointFactories = provider.GetServices(typeof(Func<EndpointDefinition>)).Cast<Func<EndpointDefinition>>().ToList();

                if (registeredEndpointFactories?.Any() == true)
                {
                    foreach (var endpointFactory in registeredEndpointFactories)
                    {
                        result.Add(endpointFactory);
                    }
                }

                return result;
            });

            services.ConfigureWithDependencies<MvcOptions, ApiControllerConvention>((mvcOptions, convention) =>
            {
                mvcOptions.Conventions.Add(convention);
                mvcOptions.Filters.Add(new ApiConfigurationActionFilter());
                mvcOptions.Filters.Add(typeof(ApiPolicyActionFilter));
            });

            services.ConfigureWithDependencies<MvcOptions, ApiActionConvention>((mvcOptions, convention) =>
            {
                mvcOptions.Conventions.Add(convention);
            });

            services.AddSingleton<IFileStreamResultConverter, FileInfoFileStreamResultConverter>();
            services.AddSingleton<IFileStreamResultConverter, FileResponseFileStreamResultConverter>();

            services.AddSingleton<FileResultFilter>();

            services.ConfigureWithDependencies<MvcOptions, FileResultFilter>((mvcOptions, filter) =>
            {
                mvcOptions.Filters.Add(filter);
            });

            services.AddDistributedMemoryCache();
            services.TryAddSingleton<IApiCache, DefaultApiCache>();
            services.TryAddSingleton<IEndpointCache, DefaultEndpointCache>();

            TryAddStartupTasks(services);

            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }

            return builder;
        }

        private static void TryAddStartupTasks(IServiceCollection services)
        {
            if (services.Any(x => x.ServiceType == typeof(IStartupTaskQueue)))
            {
                return;
            }

            services.AddStartupTasks(true,
                new StartupTasksHealthCheckParameters() { HealthCheckName = "Api Framework startup tasks" });
        }

        public static IApiFrameworkBuilder AddEndpoint(this IApiFrameworkBuilder builder, string route, ApiDefinition api, object configuration = null,
            IHealthCheck healthCheck = null, string groupName = null)
        {
            var endpointDefinition = new EndpointDefinition(route, api, configuration, endpoint => Task.FromResult(healthCheck), groupName);
            builder.AddEndpoint(endpointDefinition);

            return builder;
        }

        public static IApiFrameworkBuilder AddEndpoint(this IApiFrameworkBuilder builder, EndpointDefinition endpointDefinition)
        {
            builder.Services.AddSingleton(endpointDefinition);

            return builder;
        }
        
        public static IApiFrameworkBuilder AddEndpoint<TApiType>(this IApiFrameworkBuilder builder, string route, object configuration)
        {
            var endpointDefinition = new EndpointDefinition(route, null, configuration);

            return builder.AddEndpoint<TApiType>(endpointDefinition);
        }
        
        public static IApiFrameworkBuilder AddEndpoint<TApiType>(this IApiFrameworkBuilder builder, EndpointDefinition endpointDefinition)
        {
            builder.Services.AddSingleton(provider =>
            {
                return new Func<EndpointDefinition>(() =>
                {
                    if (endpointDefinition.Api == null)
                    {
                        var apiProvider = provider.GetRequiredService<IApiByAssemblyProvider>();
                        var api = apiProvider.GetApiByType(typeof(TApiType));

                        endpointDefinition.Api = api;
                    }

                    return endpointDefinition;
                });
            });

            return builder;
        }
    }
}
