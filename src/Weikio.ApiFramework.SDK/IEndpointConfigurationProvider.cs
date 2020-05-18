using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions.DependencyInjection;

namespace Weikio.ApiFramework.SDK
{
    public interface IEndpointConfigurationProvider
    {
        Task<List<EndpointDefinition>> GetEndpointConfiguration();
    }

    public static class ApiFrameworkBuilderPluginExtensions
    {
        public static IApiFrameworkBuilder RegisterPlugin(this IApiFrameworkBuilder apiFrameworkBuilder, Assembly pluginAssembly)
        {
            apiFrameworkBuilder.Services.RegisterPlugin(pluginAssembly);

            return apiFrameworkBuilder;
        }
        
        public static IServiceCollection RegisterPlugin(this IServiceCollection services, Assembly pluginAssembly)
        {
            services.Configure<ApiPluginOptions>(options =>
            {
                if (options.ApiPluginAssemblies.Contains(pluginAssembly))
                {
                    return;
                }
                
                options.ApiPluginAssemblies.Add(pluginAssembly);
            });
            
            return services;
        }

        public static IApiFrameworkBuilder RegisterEndpoint(this IApiFrameworkBuilder apiFrameworkBuilder, string route, object configuration = null, IHealthCheck healthCheck = null, string group = null)
        {
            apiFrameworkBuilder.Services.RegisterEndpoint(route, configuration, healthCheck, group);

            return apiFrameworkBuilder;
        }
        
        public static IServiceCollection RegisterEndpoint(this IServiceCollection services, string route, object configuration = null, IHealthCheck healthCheck = null, string group = null)
        {
            var endpointDefinition = new EndpointDefinition(route, "Weikio.ApiFramework.Plugins.Soap", configuration, healthCheck, group);
            services.AddSingleton<IEndpointConfigurationProvider>(provider => new PluginEndpointConfigurationProvider(endpointDefinition));
            
            return services;
        }
    }
}
