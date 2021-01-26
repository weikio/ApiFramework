using System;
using System.Collections.Generic;
using System.Linq;
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
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.AsyncStream;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.ApiFramework.Core.StartupTasks;
using Weikio.ApiFramework.SDK;
using Weikio.AspNetCore.Common;
using Weikio.AspNetCore.StartupTasks;

namespace Weikio.ApiFramework.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddApiFrameworkCore(this IServiceCollection services,
            Action<ApiFrameworkOptions> setupAction = null)
        {
            var builder = new ApiFrameworkBuilder(services);

            services.TryAddSingleton(provider =>
            {
                var configurationOptions = provider.GetService<IOptions<ApiFrameworkOptions>>();
                var options = configurationOptions != null ? configurationOptions.Value : new ApiFrameworkOptions();

                if (options.ApiProvider == null)
                {
                    return new EmptyApiProvider();
                }

                return options.ApiProvider;
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
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, ApiFrameworkStartupFilter>());
            services.TryAddSingleton<ApiChangeNotifier>();
            services.TryAddSingleton<EndpointConfigurationManager>();

            services.TryAddSingleton<EndpointManager>();
            services.TryAddTransient(ctx => ctx.GetService<EndpointManager>().Endpoints);
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
                        endpoint.Configuration, endpoint.HealthCheck, endpoint.GroupName);

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

                return result;
            });

            services.ConfigureWithDependencies<MvcOptions, ApiControllerConvention>((mvcOptions, convention) =>
            {
                mvcOptions.Conventions.Add(convention);
                mvcOptions.Filters.Add(new ApiConfigurationActionFilter());
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

        public static IApiFrameworkBuilder AddEndpoint(this IApiFrameworkBuilder builder, string route, string api, object configuration = null,
            IHealthCheck healthCheck = null, string groupName = null)
        {
            builder.Services.AddTransient(services =>
            {
                var endpointConfiguration = new EndpointDefinition(route, api, configuration, healthCheck, groupName);

                return endpointConfiguration;
            });

            return builder;
        }

        //public static (FunctionCatalog, EndpointCollection) AddFunctionFrameworkPlugins(
        //    this IServiceCollection services, IConfiguration configuration, FunctionFrameworkOptions options)
        //{
        //    //var functionFrameworkSection = configuration.GetSection("ApiFramework");
        //    //var hasConfig = functionFrameworkSection?.GetChildren()?.Any() == true;

        //    //if (hasConfig == false)
        //    //{
        //    //    if (options.RequireConfiguration)
        //    //    {
        //    //        throw new InvalidOperationException($"ApiFramework section is not defined.");
        //    //    }

        //    //    options.UseConfiguration = false;
        //    //}
        //    //else
        //    //{
        //    //    options.UseConfiguration = true;
        //    //}

        //    //var endpointsSection = functionFrameworkSection?.GetSection("Endpoints");
        //    //if (endpointsSection?.GetChildren()?.Any() != true && options.UseConfiguration)
        //    //{
        //    //    throw new InvalidOperationException($"Endpoints section is not defined.");
        //    //}

        //    //var pluginManager = new PluginManager();
        //    //if (options.AutoResolveFunctions)
        //    //{
        //    //    var binDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    //    pluginManager.InstallPlugins(binDirectory);
        //    //}

        //    //if (options.FunctionAssemblies?.Any() == true)
        //    //{
        //    //    pluginManager.InstallPlugins(options.FunctionAssemblies);
        //    //}

        //    //services.AddSingleton(pluginManager);
        //    //services.AddTransient(serviceProvider => pluginManager.Functions);

        //    //var changeNotifier = new FunctionChangeNotifier();
        //    //services.AddSingleton(changeNotifier);

        //    //var endpointManager = new EndpointManager(services, pluginManager.Functions, changeNotifier);
        //    //services.AddSingleton(endpointManager);
        //    //services.AddTransient(serviceProvider => endpointManager.Endpoints);

        //    //var configurationMonitor = new ConfigurationMonitor(functionFrameworkSection, endpointManager);
        //    //services.AddSingleton(configurationMonitor);

        //    //var functionChangeProvider = new FunctionChangeProvider(() => configurationMonitor.GetReloadToken());
        //    //services.AddSingleton<IActionDescriptorChangeProvider>(functionChangeProvider);
        //    //services.AddSingleton<IActionDescriptorChangeProvider>(FunctionDescriptorChangeProvider.Instance);

        //    //if (options.UseConfiguration)
        //    //{
        //    //    foreach (var endpointSection in endpointsSection.GetChildren())
        //    //    {
        //    //        endpointManager.AddEndpoint(endpointSection);
        //    //        configurationMonitor.SetEndpointConfigHashCode(endpointSection);
        //    //    }
        //    //}
        //    //else if (options.AutoResolveEndpoints)
        //    //{
        //    //    foreach (var plugin in pluginManager.Functions)
        //    //    {
        //    //        var customEndpoint = new Endpoint(options.FunctionAddressBase, plugin);
        //    //        customEndpoint.Initialize(null);

        //    //        endpointManager.AddEndpoint(customEndpoint);
        //    //    }
        //    //}

        //    //if (options.Endpoints?.Any() == true)
        //    //{
        //    //    foreach (var endpoint in options.Endpoints)
        //    //    {
        //    //        var plugin = pluginManager.Functions.FirstOrDefault(x => string.Equals(x.Name, endpoint.ApiAssemblyName));
        //    //        if (plugin == null)
        //    //        {
        //    //            throw new InvalidOperationException($"Couldn't locate function assembly {endpoint.ApiAssemblyName} for endpoint {endpoint.Route}");
        //    //        }

        //    //        var route = endpoint.Route;
        //    //        if (route.EndsWith("/"))
        //    //        {
        //    //            route = route.Remove(route.Length - 1);
        //    //        }

        //    //        if (!route.StartsWith("/"))
        //    //        {
        //    //            route = route.Insert(0, "/");
        //    //        }

        //    //        var baseRoute = options.FunctionAddressBase;
        //    //        if (baseRoute.EndsWith("/"))
        //    //        {
        //    //            baseRoute = baseRoute.Remove(baseRoute.Length - 1);
        //    //        }

        //    //        var customEndpoint = new Endpoint(baseRoute + route, plugin);
        //    //        customEndpoint.Initialize(null);

        //    //        endpointManager.AddEndpoint(customEndpoint);
        //    //    }
        //    //}

        //    //return null;
        //    //return (pluginManager.Functions, endpointManager.Endpoints);
        //}
    }
}
