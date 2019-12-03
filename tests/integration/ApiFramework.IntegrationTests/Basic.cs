using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    public class BasicTests : IClassFixture<WebApplicationFactory<CodeConfiguration.Startup>>
    {
        private readonly WebApplicationFactory<CodeConfiguration.Startup> _factory;

        public BasicTests(WebApplicationFactory<CodeConfiguration.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task MyTest()
        {
            var client = _factory.CreateClient();
            
            var response = await client.GetAsync("/swagger");
            
            response.EnsureSuccessStatusCode();
        }
        
        [Fact]
        public async Task MyTest2()
        {
            var client = _factory.CreateClient();
            
            var response = await client.GetAsync("/myapi/mytest");
            
            response.EnsureSuccessStatusCode();
        }
    }
}
