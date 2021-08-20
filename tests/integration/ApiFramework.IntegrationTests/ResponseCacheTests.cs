using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.ResponceCache;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class ResponseCacheTests : ApiFrameworkTestBase
    {

        public ResponseCacheTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanUseEndpointResponseCache()
        {
            var counterService = new CounterService(initialValue: 1);

            var server = Init(builder =>
                {
                    builder.AddApi(typeof(Counter));
                    builder.AddEndpoint("/counter01", "HelloWorld.Counter");
                    builder.AddApiFrameworkResponseCache(cacheOptions =>
                    {
                        cacheOptions.EndpointResponceCacheConfigurations.Add("/counter01", new EndpointResponceCacheConfiguration("/counter01")
                        {
                            ResponseCacheConfiguration = new ResponseCacheConfiguration(TimeSpan.FromSeconds(10), null, varyByQueryKeys: null)
                        });
                    });
                },
                configureServices: services =>
                {
                    services.AddSingleton(counterService);
                });

            Assert.Equal("1", await server.GetStringAsync("/api/counter01/getvalue"));
            Assert.Equal("1", await server.GetStringAsync("/api/counter01/getvalue"));
            Assert.Equal("1", await server.GetStringAsync("/api/counter01/getvalue"));

            // counter is increased by one when the URL path changes, but the factor parameter doesn't affect the cache because varyByQueryKeys is not set

            Assert.Equal("20", await server.GetStringAsync("/api/counter01/getmultiplied?factor=10"));
            Assert.Equal("20", await server.GetStringAsync("/api/counter01/getmultiplied?factor=100"));
            Assert.Equal("20", await server.GetStringAsync("/api/counter01/getmultiplied?factor=1000"));
        }

        [Fact]
        public async Task CanUseEndpointResponseCacheVariedByQueryKeys()
        {
            var counterService = new CounterService(initialValue: 1);

            var server = Init(builder =>
                {
                    builder.AddApi(typeof(Counter));
                    builder.AddEndpoint("/counter02", "HelloWorld.Counter");
                    builder.AddApiFrameworkResponseCache(cacheOptions =>
                    {
                        cacheOptions.EndpointResponceCacheConfigurations.Add("/counter02", new EndpointResponceCacheConfiguration("/counter02")
                        {
                            ResponseCacheConfiguration = new ResponseCacheConfiguration(TimeSpan.FromSeconds(10), null, varyByQueryKeys: new[] { "factor" })
                        });
                    });
                },
                configureServices: services =>
                {
                    services.AddSingleton(counterService);
                });

            // counter should increase by one every time the query key ("factor") changes.

            Assert.Equal("10", await server.GetStringAsync("/api/counter02/getmultiplied?factor=10"));
            Assert.Equal("10", await server.GetStringAsync("/api/counter02/getmultiplied?factor=10"));

            Assert.Equal("200", await server.GetStringAsync("/api/counter02/getmultiplied?factor=100"));
            Assert.Equal("200", await server.GetStringAsync("/api/counter02/getmultiplied?factor=100"));
            
            Assert.Equal("3000", await server.GetStringAsync("/api/counter02/getmultiplied?factor=1000"));
            Assert.Equal("3000", await server.GetStringAsync("/api/counter02/getmultiplied?factor=1000"));
        }
    }
}
