using System;
using System.Net.Http;
using System.Threading;
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
            });
            return server;
        }

        [Fact]
        public async Task SetAndRetrieveString()
        {
            var cacheValue = Guid.NewGuid().ToString();

            var server = GetClient();
            await server.GetStringAsync($"/api/cache/setstring?key=SetAndRetrieve&value={cacheValue}");
            var resultGet = await server.GetStringAsync("/api/cache/getstring?key=SetAndRetrieve");

            Assert.Equal(cacheValue, resultGet);
        }

        [Fact]
        public async Task CreateAndRetrieveString()
        {
            var cacheValue = Guid.NewGuid().ToString();
            
            var server = GetClient();
            await server.PostAsync($"/api/cache/createstring?key=CreateAndRetrieve&value={cacheValue}", null);
            var resultGet = await server.GetStringAsync("/api/cache/getstring?key=CreateAndRetrieve");

            Assert.Equal(cacheValue, resultGet);
        }

        [Fact]
        public async Task CreateAndRetrieveAsyncronousString()
        {
            var cacheValue = Guid.NewGuid().ToString();

            var server = GetClient();
            await server.PostAsync($"/api/cache/createasyncronousstring?key=CreateAndRetrieveAsyncronous&value={cacheValue}", null);
            var resultGet = await server.GetStringAsync("/api/cache/getstring?key=CreateAndRetrieveAsyncronous");

            Assert.Equal(cacheValue, resultGet);
        }

        [Fact]
        public async Task SetAndRemoveString()
        {
            var cacheKey = "SetAndRemoveString";
            var cacheValue = Guid.NewGuid().ToString();

            var server = GetClient();
            await server.GetStringAsync($"/api/cache/setstring?key={cacheKey}&value={cacheValue}");

            var firstQueryResult = await server.GetStringAsync($"/api/cache/getstring?key={cacheKey}");
            Assert.Equal(cacheValue, firstQueryResult);

            await server.GetAsync($"/api/cache/itemremove?key={cacheKey}");

            var secondQueryResult = await server.GetStringAsync($"/api/cache/getstring?key={cacheKey}");
            Assert.Empty(secondQueryResult);
        }

        [Fact]
        public async Task CreateAndRetrieveBytes()
        {
            var cacheValue = Guid.NewGuid().ToString();

            var server = GetClient();
            await server.PostAsync($"/api/cache/setstringbytes?key=CreateAndRetrieveBytes&value={cacheValue}", null);
            var resultGet = await server.GetStringAsync("/api/cache/getstringfrombytes?key=CreateAndRetrieveBytes");

            Assert.Equal(cacheValue, resultGet);
        }

        [Fact]
        public async Task StringNotSet()
        {
            var server = GetClient();
            var resultGet = await server.GetStringAsync("/api/cache/getstring?key=unknownkey");

            Assert.Empty(resultGet);
        }

        [Fact]
        public async Task Timeout()
        {
            var cacheKey = "Timeout";
            var cacheValue = Guid.NewGuid().ToString();
            var expirationTimeInSeconds = 1;

            var server = GetClient();
            await server.GetStringAsync($"/api/cache/setslidingstring?key={cacheKey}&value={cacheValue}&slidingExpirationInSeconds={expirationTimeInSeconds}");

            var firstQueryResult = await server.GetStringAsync($"/api/cache/getstring?key={cacheKey}");
            Assert.Equal(cacheValue, firstQueryResult);

            Thread.Sleep((int)TimeSpan.FromSeconds(expirationTimeInSeconds).TotalMilliseconds);

            var secondQueryResult = await server.GetStringAsync($"/api/cache/getstring?key={cacheKey}");
            Assert.Empty(secondQueryResult);
        }

        [Fact]
        public async Task SlidingTimeout()
        {
            var cacheKey = "SlidingTimeout";
            var cacheValue = Guid.NewGuid().ToString();
            var expirationTimeInSeconds = 1;

            var server = GetClient();
            await server.GetStringAsync($"/api/cache/setslidingstring?key={cacheKey}&value={cacheValue}&slidingExpirationInSeconds={expirationTimeInSeconds}");

            var firstQueryResult = await server.GetStringAsync($"/api/cache/getstring?key={cacheKey}");
            Assert.Equal(cacheValue, firstQueryResult);

            Thread.Sleep(750);

            // refresh sliding expiration
            await server.GetStringAsync($"/api/cache/itemrefresh?key={cacheKey}");

            Thread.Sleep(750);

            var secondQueryResult = await server.GetStringAsync($"/api/cache/getstring?key={cacheKey}");
            Assert.Equal(cacheValue, secondQueryResult);

            Thread.Sleep((int)TimeSpan.FromSeconds(expirationTimeInSeconds).TotalMilliseconds);

            var thirdQueryResult = await server.GetStringAsync($"/api/cache/getstring?key={cacheKey}");
            Assert.Empty(thirdQueryResult);
        }

        [Fact]
        public async Task SeparateEndPoints()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldCacheApi));
                builder.AddEndpoint<HelloWorldCacheApi>("/cache1");
                builder.AddEndpoint<HelloWorldCacheApi>("/cache2");
            });

            var cacheKey = "SeparateEndPoints";
            var cacheValue = Guid.NewGuid().ToString();
            
            //set with first end point
            await server.GetStringAsync($"/api/cache1/setstring?key={cacheKey}&value={cacheValue}");
            //retrieve from another
            var resultGet = await server.GetStringAsync($"/api/cache2/getstring?key={cacheKey}");

            Assert.Empty(resultGet);
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
                    options.GetKey = (endpoint, provider, key) => { return key; };
                });
                services.Configure(apiCacheOptions);
            });

            var cacheValue = Guid.NewGuid().ToString();

            //set with first end point
            await server.GetStringAsync($"/api/cache1/setstring?key=SeparateEndPointsWithSameKey&value={cacheValue}");
            //retrieve from another
            var resultGet = await server.GetStringAsync("/api/cache2/getstring?key=SeparateEndPointsWithSameKey");

            Assert.Equal(cacheValue, resultGet);
        }
    }
}
