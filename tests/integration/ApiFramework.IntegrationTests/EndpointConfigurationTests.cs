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
    public class EndpointConfigurationTests : ApiFrameworkTestBase
    {
        public EndpointConfigurationTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanSetConfiguration()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldConfigurationApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldConfigurationApi", "TestConfiguration");
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
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldComplexConfigurationApi", configuration);
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
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldComplexConfigurationApi", new {Name = "anonymous", Country = "mycountry", Age = 40});
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");
            
            // Assert
            var expected = $"anonymous-mycountry-{40}";
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task CanSetDifferentConfigurationToDifferentEndpoints()
        {
            var firstConfiguration = new Complex() { Country = "hello", Name = "there", Age = 50 };
            var secondConfiguration = new Complex() { Country = "another", Name = "here", Age = 70 };

            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldComplexConfigurationApi));
                builder.AddEndpoint("/first", "HelloWorld.HelloWorldComplexConfigurationApi", firstConfiguration);
                builder.AddEndpoint("/second", "HelloWorld.HelloWorldComplexConfigurationApi", secondConfiguration);
            });

            // Act 
            var firstResult = await server.GetStringAsync("/api/first");
            var secondResult = await server.GetStringAsync("/api/second");
            
            // Assert
            var firstExpected = $"{firstConfiguration.Name}-{firstConfiguration.Country}-{firstConfiguration.Age}";
            var secondExpected = $"{secondConfiguration.Name}-{secondConfiguration.Country}-{secondConfiguration.Age}";

            Assert.Equal(firstExpected, firstResult);
            Assert.Equal(secondExpected, secondResult);
        }
    }
}
