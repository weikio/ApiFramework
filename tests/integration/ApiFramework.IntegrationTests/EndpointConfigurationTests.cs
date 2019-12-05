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
        public async Task CanSetDifferentConfigurationToDifferentEndpoints()
        {
            var firstConfiguration = new Complex() { Country = "hello", Name = "there", Age = 50 };
            var secondConfiguration = new Complex() { Country = "another", Name = "here", Age = 70 };

            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldComplexConfigurationApi));
                builder.AddEndpoint("/first", "HelloWorld", firstConfiguration);
                builder.AddEndpoint("/second", "HelloWorld", secondConfiguration);
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

    public class EndpointFactoryTests : ApiFrameworkTestBase
    {
        public EndpointFactoryTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }
        
        
        [Fact]
        public async Task CanSetRouteToConfiguration()
        {
            throw new NotImplementedException();
        }
    }
}
