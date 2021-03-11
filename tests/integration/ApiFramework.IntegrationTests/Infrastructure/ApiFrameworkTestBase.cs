using System;
using System.Net.Http;
using CodeConfiguration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.ApiFramework.AspNetCore;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public abstract class ApiFrameworkTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        
        // Must be set in each test
        public ITestOutputHelper Output { get; set; }
        
        protected ApiFrameworkTestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }
        
        protected HttpClient Init(Action<IApiFrameworkBuilder> action, Action<ApiFrameworkAspNetCoreOptions> setupAction = null, Action<IServiceCollection> configureServices = null)
        {
            var result = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var apiFrameworkBuilder = services.AddApiFramework(options =>
                    {
                        options.AutoResolveApis = false;
                        options.AutoResolveEndpoints = false;

                        setupAction?.Invoke(options);
                    });
                    
                    action(apiFrameworkBuilder);
                    
                    configureServices?.Invoke(services);
                });
                
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove other loggers
                    logging.AddXUnit(Output); // Use the ITestOutputHelper instance
                });
            }).CreateClient();

            return result;
        }
    }
}
