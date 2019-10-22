using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.AspNetCore.Common;
using Weikio.AspNetCore.StartupTasks;

namespace Weikio.ApiFramework.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IFunctionFrameworkBuilder AddFunctionFrameworkCore(this IServiceCollection services,
            IMvcBuilder mvcBuilder, Action<FunctionFrameworkOptions> setupAction = null)
        {
//            var options = new FunctionFrameworkOptions();
//            if (setupAction != null)
//            {
//                setupAction(options);
//            }
//            
            var builder = new FunctionFrameworkBuilder(services);

//
//            FunctionHttpVerbResolver.GetHttpVerb = options.HttpVerbResolver;
//            FunctionLocator.IsFunction = options.FunctionResolver;

//            if (options.FunctionProvider != null)
//            {
//                services.AddSingleton(options.FunctionProvider);
//            }
//            else if (options.FunctionProvider == null && options.FunctionAssemblies?.Any() == true)
//            {
//                var assemblyCatalogs = new List<IPluginCatalog>();
//                foreach (var functionAssembly in options.FunctionAssemblies)
//                {
//                    var assemblyCatalog = new AssemblyPluginCatalog(functionAssembly);
//                    assemblyCatalogs.Add(assemblyCatalog);
//                }
//
//                var compositeCatalog = new CompositePluginCatalog(assemblyCatalogs.ToArray());
//                var functionProvider = new PluginFrameworkFunctionProvider(compositeCatalog);
//
//                services.AddSingleton<IFunctionProvider>(functionProvider);
//            }
//            else if (options.FunctionProvider == null && options.AutoResolveFunctions)
//            {
//                var binDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
//                var pluginCatalog = new FolderPluginCatalog(binDirectory, FunctionLocator.IsFunction);
//
//                var functionProvider = new PluginFrameworkFunctionProvider(pluginCatalog);
//                services.AddSingleton<IFunctionProvider>(functionProvider);
//            }

            services.AddTransient<IStartupFilter, FunctionFrameworkStartupFilter>();

//            services.AddSingleton<PluginManager>();
//            services.AddTransient(ctx => ctx.GetService<PluginManager>().Functions);
            services.AddSingleton<FunctionChangeNotifier>();
            services.AddSingleton<EndpointConfigurationManager>();

            services.AddSingleton<EndpointManager>();
            services.AddTransient(ctx => ctx.GetService<EndpointManager>().Endpoints);
            services.AddSingleton<EndpointInitializer>();

            //var configurationMonitor = new ConfigurationMonitor(functionFrameworkSection, endpointManager);
//            services.AddSingleton<ConfigurationMonitor>();

//            services.AddSingleton<IActionDescriptorChangeProvider>(ctx =>
//            {
////                var configManager = ctx.GetRequiredService<ConfigurationMonitor>();
//
//                return new FunctionChangeProvider(() => configManager.GetReloadToken());
//            });

//            services.AddSingleton<IActionDescriptorChangeProvider>(FunctionDescriptorChangeProvider.Instance);
//            services.AddSingleton<FunctionDescriptorChangeProvider>();
//            var wellKnownChangeToken = new WellKnownChangeToken();
            services.AddSingleton<FunctionChangeToken>();
            services.AddSingleton<IActionDescriptorChangeProvider, ActionDescriptorChangeProvider>();

            services.AddHttpContextAccessor();

            //var (plugins, pluginEndpoints) = services.AddFunctionFrameworkPlugins(configuration, options);

            services.AddSingleton<FunctionControllerConvention>();
            services.AddSingleton<FunctionActionConvention>();
            services.AddSingleton<FunctionFeatureProvider>();

            // Services for running background tasks like endpoint initializations
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<StatusProvider>();
            services.AddSingleton<HealthProbe>();

            services.AddHealthChecks()
                .AddCheck<EndpointHeathCheck>("function_framework_endpoint", HealthStatus.Degraded, new[] { "function_framework_endpoint" });

            builder.Services.AddSingleton<IHealthCheckPublisher, HealthPublisher>();

            services.AddSingleton<IEndpointConfigurationProvider>(provider =>
            {
                var result = new CodeBasedEndpointConfigurationProvider();
                var configurationOptions = provider.GetService<IOptions<FunctionFrameworkOptions>>();
                FunctionFrameworkOptions options = null;

                if (configurationOptions != null)
                {
                    options = configurationOptions.Value;
                }
                else
                {
                    options = new FunctionFrameworkOptions();
                }

                foreach (var endpoint in options.Endpoints)
                {
                    var endpointConfiguration = new EndpointConfiguration(endpoint.Route, endpoint.FunctionAssemblyName,
                        endpoint.Configuration, endpoint.HealthCheck);

                    result.Add(endpointConfiguration);
                }

                var registeredEndpoints = provider.GetServices(typeof(EndpointConfiguration)).Cast<EndpointConfiguration>().ToList();

                if (registeredEndpoints?.Any() == true)
                {
                    foreach (var endpointConfiguration in registeredEndpoints)
                    {
                        result.Add(endpointConfiguration);
                    }
                }

                return result;
            });

            services.ConfigureWithDependencies<MvcOptions, FunctionControllerConvention>((mvcOptions, convention) =>
            {
                mvcOptions.Conventions.Add(convention);
            });

            services.ConfigureWithDependencies<MvcOptions, FunctionActionConvention>((mvcOptions, convention) =>
            {
                mvcOptions.Conventions.Add(convention);
            });

//            services.AddSingleton(options);
            TryAddStartupTasks(services);

            //services.ConfigureWithDependencies<MvcOptions, FunctionActionConvention>((mvcOptions, convention) =>
            //{
            //    //mvcOptions.
            //    //mvcOptions.Conventions.Add(convention);
            //});

            mvcBuilder.AddMvcOptions(o =>
            {
                o.Filters.Add(new FunctionConfigurationActionFilter());
            });

            //mvcBuilder.ConfigureApplicationPartManager(m => m.AddFunctionFrameworkFeatures(() => services.BuildServiceProvider().GetRequiredService<EndpointManager>()));

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
                new StartupTasksHealthCheckParameters() { HealthCheckName = "Function Framework startup tasks" });
        }

        //public static (FunctionCatalog, EndpointCollection) AddFunctionFrameworkPlugins(
        //    this IServiceCollection services, IConfiguration configuration, FunctionFrameworkOptions options)
        //{
        //    //var functionFrameworkSection = configuration.GetSection("FunctionFramework");
        //    //var hasConfig = functionFrameworkSection?.GetChildren()?.Any() == true;

        //    //if (hasConfig == false)
        //    //{
        //    //    if (options.RequireConfiguration)
        //    //    {
        //    //        throw new InvalidOperationException($"FunctionFramework section is not defined.");
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
        //    //        var plugin = pluginManager.Functions.FirstOrDefault(x => string.Equals(x.Name, endpoint.FunctionAssemblyName));
        //    //        if (plugin == null)
        //    //        {
        //    //            throw new InvalidOperationException($"Couldn't locate function assembly {endpoint.FunctionAssemblyName} for endpoint {endpoint.Route}");
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
