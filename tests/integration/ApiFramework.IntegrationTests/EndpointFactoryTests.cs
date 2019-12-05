using System;
using System.Threading.Tasks;
using AnotherHelloWorld;
using CodeConfiguration;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;
using Xunit;
using ApiFactory = HelloWorld.ApiFactory;

namespace ApiFramework.IntegrationTests
{
    public class EndpointFactoryTests : ApiFrameworkTestBase
    {
        public EndpointFactoryTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }
        
        [Fact]
        public async Task CanCreateApiFromFactory()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(ApiFactory));
                builder.AddEndpoint("/mytest", "HelloWorld");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello Api Framework!", result);        
        }
        
        [Fact]
        public async Task ApiCanSupportFactoryAndStatic()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(AnotherHelloWorldApi).Assembly);
                builder.AddEndpoint("/mytest", "AnotherHelloWorld");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest/AnotherHelloWorld");
            var factoryResult = await server.GetStringAsync("/api/mytest/TestFunctionality");
            
            // Assert
            Assert.Equal("Hello Api Framework!", result);        
            Assert.Equal("Hello from Factory", factoryResult);        
        }
        
        [Fact]
        public async Task FactoryApiDoesNotCareAboutApiNaming()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(AnotherHelloWorld.ApiFactory));
                builder.AddEndpoint("/mytest", "AnotherHelloWorld");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello from Factory", result);        
        }
        
        [Fact]
        public async Task CanCreateApiWithConfiguration()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task CanUseEndpointConfiguration()
        {
            throw new NotImplementedException();
        }
        
        [Fact]
        public async Task CanUseRouteParameter()
        {
            throw new NotImplementedException();
        }
    }
}
