using System.Net;
using System.Threading.Tasks;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class EndpointTests : ApiFrameworkTestBase
    {
        public EndpointTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanAddEndpointToApi()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                builder.AddEndpoint("/thisistheendpoint", "HelloWorld.HelloWorldApi");
            });

            var result = await server.GetAsync("/api/thisistheendpoint");

            Assert.True(result.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CanAddMultipleEndpointsToApi()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                builder.AddEndpoint("/first", "HelloWorld.HelloWorldApi");
                builder.AddEndpoint("/second", "HelloWorld.HelloWorldApi");
                builder.AddEndpoint("/third", "HelloWorld.HelloWorldApi");
            });

            var firstResult = await server.GetAsync("/api/first");
            Assert.True(firstResult.IsSuccessStatusCode);

            var secondResult = await server.GetAsync("/api/second");
            Assert.True(secondResult.IsSuccessStatusCode);

            var thirdResult = await server.GetAsync("/api/third");
            Assert.True(thirdResult.IsSuccessStatusCode);

            var missing = await server.GetAsync("/api/notexists");
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        }
    }
}
