using System.Threading.Tasks;
using ApiFramework.IntegrationTests.Infrastructure;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Weikio.ApiFramework;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class ControllerApiTests : ApiFrameworkTestBase
    {
        public ControllerApiTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }
        
        [Fact]
        public async Task CanCreateApiFromController()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(SampleController));
                builder.AddEndpoint("/mytest", "HelloWorld.SampleController");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest/Sample/single");
            var obj = JObject.Parse(result);
            
            // Assert
            Assert.Equal("Test", obj["Summary"].Value<string>());
        }
    }
}
