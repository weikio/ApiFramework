using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ApiFramework.IntegrationTests.Infrastructure;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class PolicyTests : ApiFrameworkTestBase
    {
        public PolicyTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanAddPolicyToEndpoint()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                var endpoint = new EndpointDefinition("/policytest", "HelloWorld.HelloWorldApi", policyName: "testPolicy");
                
                builder.AddEndpoint(endpoint);
            });
            
            var result = await server.GetAsync("/api/policytest");
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }
        
        [Fact]
        public async Task CanCallEndpointIfPolicyMet()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                var endpoint = new EndpointDefinition("/policytest", "HelloWorld.HelloWorldApi", policyName: "testPolicy");
                
                builder.AddEndpoint(endpoint);
            });
            
            server.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "test");
            var result = await server.GetAsync("/api/policytest");
            
            Assert.True(result.IsSuccessStatusCode);
        }
        
        [Fact]
        public async Task ByDefaultNoPolicy()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                var endpoint1 = new EndpointDefinition("/policytest", "HelloWorld.HelloWorldApi", policyName: "testPolicy");
                var endpoint2 = new EndpointDefinition("/nopolicy", "HelloWorld.HelloWorldApi");
                
                builder.AddEndpoint(endpoint1);
                builder.AddEndpoint(endpoint2);
            });
                        
            var result1 = await server.GetAsync("/api/policytest");
            Assert.Equal(HttpStatusCode.Forbidden, result1.StatusCode);
            
            var result2 = await server.GetAsync("/api/nopolicy");
            Assert.True(result2.IsSuccessStatusCode);
        }
    }
}
