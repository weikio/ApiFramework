using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CodeConfiguration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.ApiFramework.AspNetCore;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests.Infrastructure
{
    public abstract class ApiFrameworkTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        
        // Must be set in each test
        public ITestOutputHelper Output { get; set; }
        public IServiceProvider Provider { get; set; }
        
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
            });

            Provider = result.Services;
            return result.CreateClient();
        }
        
        protected async Task ContinueWhen(Func<bool> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromSeconds(3);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout.GetValueOrDefault());

            var success = false;

            while (cts.IsCancellationRequested == false)
            {
                success = probe();

                if (success)
                {
                    break;
                }

                if (cts.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
        }
        
        protected async Task ContinueWhen(Func<Task<bool>> probe, string assertErrorMessage = null, TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.FromSeconds(3);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout.GetValueOrDefault());

            var success = false;

            while (cts.IsCancellationRequested == false)
            {
                success = await probe();

                if (success)
                {
                    break;
                }

                if (cts.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
        }
    }
}
