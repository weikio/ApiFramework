using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Abstractions.DependencyInjection;

namespace Weikio.ApiFramework.SDK
{
    public static class ApiFrameworkBuilderPluginExtensions
    {
        public static IApiFrameworkBuilder RegisterPlugin(this IApiFrameworkBuilder apiFrameworkBuilder, string endpoint, object configuration = null)
        {
            var pluginAssembly = Assembly.GetCallingAssembly();
            
            apiFrameworkBuilder.Services.RegisterPlugin(pluginAssembly, endpoint, configuration);

            return apiFrameworkBuilder;
        }

        public static IServiceCollection RegisterPlugin(this IServiceCollection services, string endpoint, object configuration = null)
        {
            var pluginAssembly = Assembly.GetCallingAssembly();

            services.RegisterPlugin(pluginAssembly, endpoint, configuration);

            return services;
        }

        public static IServiceCollection RegisterPlugin(this IServiceCollection services, Assembly assembly, string endpoint, object configuration = null)
        {
            Action<EndpointDefinition> configureEndpoint = null;

            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                configureEndpoint = new Action<EndpointDefinition>(definition =>
                {
                    definition.Route = endpoint;
                    definition.Configuration = configuration;
                });
            }

            Type configurationType = null;
            if (configuration != null)
            {
                configurationType = configuration.GetType();
            }
            
            services.RegisterPlugin(assembly, configurationType, configureEndpoint);

            return services;
        }

        public static IApiFrameworkBuilder RegisterPlugin<TPluginType>(this IApiFrameworkBuilder apiFrameworkBuilder, string endpoint,
            object configuration = null)
        {
            apiFrameworkBuilder.Services.RegisterPlugin<TPluginType>(endpoint, configuration);

            return apiFrameworkBuilder;
        }

        public static IServiceCollection RegisterPlugin<TPluginType>(this IServiceCollection services, string endpoint, object configuration = null)
        {
            var configureEndpoint = new Action<EndpointDefinition>(definition =>
            {
                definition.Route = endpoint;
                definition.Configuration = configuration;
            });

            Type configurationType = null;
            if (configuration != null)
            {
                configurationType = configuration.GetType();
            }

            services.RegisterPlugin<TPluginType>(configurationType, configureEndpoint);

            return services;
        }

        public static IApiFrameworkBuilder RegisterPlugin<TPluginType>(this IApiFrameworkBuilder builder, Type configurationType = null, Action<EndpointDefinition> configureEndpoint = null)
        {
            builder.Services.RegisterPlugin<TPluginType>(configurationType, configureEndpoint);

            return builder;
        }

        public static IServiceCollection RegisterPlugin<TPluginType>(this IServiceCollection services, Type configurationType = null, Action<EndpointDefinition> configureEndpoint = null)
        {
            var assembly = typeof(TPluginType).Assembly;

            services.RegisterPlugin(assembly, configurationType, configureEndpoint);

            return services;
        }

        public static IServiceCollection RegisterPlugin(this IServiceCollection services, Assembly pluginAssembly = null, Type configurationType = null,
            Action<EndpointDefinition> configureEndpoint = null)
        {
            if (pluginAssembly == null)
            {
                throw new ArgumentNullException(nameof(pluginAssembly));
            }

            var apiPlugin = new ApiPlugin { Assembly = pluginAssembly };
            services.AddSingleton(typeof(ApiPlugin), apiPlugin);

            services.Configure<ApiPluginOptions>(options =>
            {
                if (options.ApiPluginAssemblies.Contains(pluginAssembly))
                {
                    return;
                }

                options.ApiPluginAssemblies.Add(pluginAssembly);
            });

            if (configurationType != null)
            {
                services.AddSingleton(provider =>
                {
                    var apiProvider = provider.GetRequiredService<IApiByAssemblyProvider>();
                    var api = apiProvider.GetApiByAssembly(pluginAssembly);

                    return new ApiConfiguration(api, configurationType);
                });
            }

            if (configureEndpoint != null)
            {
                services.AddSingleton(provider =>
                {
                    return new Func<EndpointDefinition>(() =>
                    {
                        var endpointDefinition = new EndpointDefinition();
                        configureEndpoint(endpointDefinition);

                        if (endpointDefinition.Api == null)
                        {
                            var apiProvider = provider.GetRequiredService<IApiByAssemblyProvider>();
                            var api = apiProvider.GetApiByAssembly(pluginAssembly);

                            endpointDefinition.Api = api;
                            // Todo: Throw if API still null
                        }

                        return endpointDefinition;
                    });
                });
            }

            return services;
        }

        public static IApiFrameworkBuilder RegisterEndpoint(this IApiFrameworkBuilder apiFrameworkBuilder, string route, string pluginName,
            object configuration = null, IHealthCheck healthCheck = null, string group = null)
        {
            apiFrameworkBuilder.Services.RegisterEndpoint(route, pluginName, configuration, healthCheck, group);

            return apiFrameworkBuilder;
        }

        public static IServiceCollection RegisterEndpoint(this IServiceCollection services, string route, string pluginName, object configuration = null,
            IHealthCheck healthCheck = null, string group = null)
        {
            var endpointDefinition = new EndpointDefinition(route, pluginName, configuration, endpoint => Task.FromResult(healthCheck), group);
            services.AddSingleton<IEndpointConfigurationProvider>(provider => new PluginEndpointConfigurationProvider(endpointDefinition));

            return services;
        }
    }
}
