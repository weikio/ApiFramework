using System;
using System.Net.Http;
using System.Threading.Tasks;
using ApiFramework.IntegrationTests.Infrastructure;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework;
using Weikio.ApiFramework.Core.Cache;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class CacheTests : ApiFrameworkTestBase
    {
        public CacheTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        private HttpClient GetClient()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldCacheApi));
                builder.AddEndpoint<HelloWorldCacheApi>("/cache");
            },
            configureServices: services => {
                var apiCacheOptions = new Action<ApiCacheOptions>(options =>
                {
                    options.ExpirationTime = TimeSpan.FromSeconds(5);
                });
                services.Configure(apiCacheOptions);
            });
            return server;
        }

        [Fact]
        public async Task SetAndRetrieveString()
        {
            var server = GetClient();
            await server.GetStringAsync("/api/cache/setstring?name=test");
            var resultGet = await server.GetStringAsync("/api/cache/getstring");

            Assert.Equal("Hello test from cache", resultGet);
        }

        [Fact]
        public async Task CreateAndRetrieveString()
        {
            var server = GetClient();
            await server.PostAsync("/api/cache/createstring?name=test", null);
            var resultGet = await server.GetStringAsync("/api/cache/getstring");

            Assert.Equal("Hello test (function) from cache", resultGet);
        }

        [Fact]
        public async Task CreateAndRetrieveAsyncronousString()
        {
            var server = GetClient();
            await server.PostAsync("/api/cache/createasyncronousstring?name=test", null);
            var resultGet = await server.GetStringAsync("/api/cache/getstring");

            Assert.Equal("Hello test (async function) from cache", resultGet);
        }


        [Fact]
        public async Task CreateAndRetrieveData()
        {
            var server = GetClient();
            await server.PostAsync("/api/cache/setdata?name=test", null);
            var resultGet = await server.GetStringAsync("/api/cache/getdata");

            Assert.Equal("Hello test data from cache", resultGet);
        }

        [Fact]
        public async Task StringNotSet()
        {
            var server = GetClient();
            var resultGet = await server.GetStringAsync("/api/cache/getstring");

            Assert.Equal("Hello. Value not found from cache", resultGet);
        }

        [Fact]
        public async Task Timeout()
        {
            var server = GetClient();
            var resultGet = await server.GetStringAsync("/api/cache/timeout?name=test&timeInSeconds=6");
            Assert.Equal("Hello. You were removed from cache", resultGet);
        }

        [Fact]
        public async Task SeparateEndPoints()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldCacheApi));
                builder.AddEndpoint<HelloWorldCacheApi>("/cache1");
                builder.AddEndpoint<HelloWorldCacheApi>("/cache2");
            },
            configureServices: services => {
                var apiCacheOptions = new Action<ApiCacheOptions>(options =>
                {
                    options.ExpirationTime = TimeSpan.FromSeconds(5);
                });
                services.Configure(apiCacheOptions);
            });
            //set with first end point
            await server.GetStringAsync("/api/cache1/setstring?name=test");
            //retrieve from another
            var resultGet = await server.GetStringAsync("/api/cache2/getstring");

            Assert.Equal("Hello. Value not found from cache", resultGet);
        }


        [Fact]
        public async Task SeparateEndPointsWithSameKey()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldCacheApi));
                builder.AddEndpoint<HelloWorldCacheApi>("/cache1");
                builder.AddEndpoint<HelloWorldCacheApi>("/cache2");
            },
            configureServices: services => {
                var apiCacheOptions = new Action<ApiCacheOptions>(options =>
                {
                    options.ExpirationTime = TimeSpan.FromSeconds(5);
                    options.GetKey = (endpoint, provider, key) => { return key; };
                });
                services.Configure(apiCacheOptions);
            });
            //set with first end point
            await server.GetStringAsync("/api/cache1/setstring?name=test");
            //retrieve from another
            var resultGet = await server.GetStringAsync("/api/cache2/getstring");

            Assert.Equal("Hello test from cache", resultGet);
        }
    }
}
