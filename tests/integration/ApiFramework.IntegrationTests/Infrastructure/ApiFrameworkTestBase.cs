using System;
using System.Net.Http;
using CodeConfiguration;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.AspNetCore;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    public abstract class ApiFrameworkTestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        protected ApiFrameworkTestBase(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
        
        protected HttpClient Init(Action<IApiFrameworkBuilder> action, Action<ApiFrameworkAspNetCoreOptions> setupAction = null)
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
                });
                
            }).CreateClient();

            return result;
        }
    }
}
