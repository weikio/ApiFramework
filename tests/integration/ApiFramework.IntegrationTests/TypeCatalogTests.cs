using System.Threading.Tasks;
using ApiFramework.IntegrationTests.Infrastructure;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.ApiFramework;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class TypeCatalogTests : ApiFrameworkTestBase
    {
        public TypeCatalogTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }
        
        [Fact]
        public async Task CanUseNonApiEndingApi()
        {
            var server = Init(builder =>
            {
                builder.AddApi<Calculator>();
                builder.AddEndpoint("/mytest", "HelloWorld.Calculator");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest?x=5&y=10");

            // Assert
            Assert.Equal("15", result);
        }
    }
}
