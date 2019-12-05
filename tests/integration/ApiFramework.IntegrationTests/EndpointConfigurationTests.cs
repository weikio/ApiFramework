using System;
using System.Threading.Tasks;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    public class EndpointConfigurationTests : ApiFrameworkTestBase
    {
        public EndpointConfigurationTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CanSetConfiguration()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldConfigurationApi));
                builder.AddEndpoint("/mytest", "HelloWorld", "TestConfiguration");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("TestConfiguration", result);
        }

        [Theory]
        [InlineData("Finland", "John", 45)]
        [InlineData("Sweden", "Jane", 50)]
        [InlineData("Norway", "June", 55)]
        public async Task CanSetComplexConfiguration(string country, string name, int age)
        {
            var configuration = new Complex() { Country = country, Name = name, Age = age };

            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldComplexConfigurationApi));
                builder.AddEndpoint("/mytest", "HelloWorld", configuration);
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");
            
            // Assert
            var expected = $"{name}-{country}-{age}";
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task CanSetAnonymousConfiguration()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldComplexConfigurationApi));
                builder.AddEndpoint("/mytest", "HelloWorld", new {Name = "anonymous", Country = "mycountry", Age = 40});
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");
            
            // Assert
            var expected = $"anonymous-mycountry-{40}";
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task CanSetRouteToConfiguration()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanSetDifferentConfigurationToDifferentEndpoints()
        {
            throw new NotImplementedException();
        }
    }
}
