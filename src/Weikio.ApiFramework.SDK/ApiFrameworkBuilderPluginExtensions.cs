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
            Func<ApiDefinition, EndpointDefinition> createEndpoint = null;

            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                createEndpoint = new Func<ApiDefinition, EndpointDefinition>(api =>
                {
                    return new EndpointDefinition(endpoint, api)
                    { 
                        Configuration = configuration
                    };
                });
            }

            Type configurationType = null;
            if (configuration != null)
            {
                configurationType = configuration.GetType();
            }
            
            services.RegisterPlugin(assembly, configurationType, createEndpoint);

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
            var configureEndpoint = new Func<ApiDefinition, EndpointDefinition>(api =>
            {
                return new EndpointDefinition(endpoint, api);
            });

            Type configurationType = null;
            if (configuration != null)
            {
                configurationType = configuration.GetType();
            }

            services.RegisterPlugin<TPluginType>(configurationType, configureEndpoint);

            return services;
        }

        public static IApiFrameworkBuilder RegisterPlugin<TPluginType>(this IApiFrameworkBuilder builder, Type configurationType = null, Func<ApiDefinition, EndpointDefinition> createEndpoint = null)
        {
            builder.Services.RegisterPlugin<TPluginType>(configurationType, createEndpoint);

            return builder;
        }

        public static IServiceCollection RegisterPlugin<TPluginType>(this IServiceCollection services, Type configurationType = null, Func<ApiDefinition, EndpointDefinition> createEndpoint = null)
        {
            var assembly = typeof(TPluginType).Assembly;

            services.RegisterPlugin(assembly, configurationType, createEndpoint);

            return services;
        }

        public static IServiceCollection RegisterPlugin(this IServiceCollection services, Assembly pluginAssembly = null, Type configurationType = null,
            Func<ApiDefinition, EndpointDefinition> createEndpoint = null)
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

            if (createEndpoint != null)
            {
                services.AddSingleton(provider =>
                {
                    var apiProvider = provider.GetRequiredService<IApiByAssemblyProvider>();
                    var api = apiProvider.GetApiByAssembly(pluginAssembly);

                    return new Func<EndpointDefinition>(() =>
                    {
                        var endpointDefinition = createEndpoint(api);

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
