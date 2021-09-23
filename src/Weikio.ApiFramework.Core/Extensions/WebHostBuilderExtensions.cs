using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.SDK;

// ReSharper disable once CheckNamespace
namespace Weikio.ApiFramework
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder AddApiFrameworkJsonConfigurationFile(this IWebHostBuilder webHostBuilder, string filePath = "apiframework.json")
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
