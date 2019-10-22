using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;

namespace Weikio.ApiFramework.Core.Extensions
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder AddFunctionFrameworkJsonConfigurationFile(this IWebHostBuilder webHostBuilder, string filePath = "functionframework.json")
        {
            webHostBuilder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddJsonFile(filePath, false, true);
            });

            webHostBuilder.ConfigureServices(services =>
            {
                services.AddTransient(typeof(IEndpointConfigurationProvider), typeof(AppConfigurationEndpointConfigurationProvider));
            });

            return webHostBuilder;
        }
    }
}
