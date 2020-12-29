using System;
using System.Threading.Tasks;
using AnotherHelloWorld;
using CodeConfiguration;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;
using Xunit;
using Xunit.Abstractions;
using ApiFactory = HelloWorld.ApiFactory;

namespace ApiFramework.IntegrationTests
{
    public class EndpointFactoryTests : ApiFrameworkTestBase
    {
        public EndpointFactoryTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanCreateApiFromStaticFactory()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(ApiFactory));
                builder.AddEndpoint("/mytest", "HelloWorld.ApiFactory");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello Api Framework!", result);
        }

        [Fact]
        public async Task CanCreateSingleApiFromFactory()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(AnotherHelloWorld.SingleNonTask.ApiFactory));
                builder.AddEndpoint("/mytest", typeof(AnotherHelloWorld.SingleNonTask.ApiFactory).FullName);
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello from Factory", result);

        }

        [Fact]
        public async Task CanCreateSingleApiFromFactoryWithAsync()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(AnotherHelloWorld.SingleTask.ApiFactory));
                builder.AddEndpoint("/mytest", typeof(AnotherHelloWorld.SingleTask.ApiFactory).FullName);
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello from Factory", result);

        }

        [Fact]
        public async Task CanCreateMultipleApiFromFactory()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(AnotherHelloWorld.MultiNonTask.ApiFactory));
                builder.AddEndpoint("/mytest", typeof(AnotherHelloWorld.MultiNonTask.ApiFactory).FullName);
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello from Factory", result);

        }

        [Fact]
        public async Task CanCreateMultipleApiFromFactoryWithAsync()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(AnotherHelloWorld.MultiTask.ApiFactory));
                builder.AddEndpoint("/mytest", typeof(AnotherHelloWorld.MultiTask.ApiFactory).FullName);

            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello from Factory", result);

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
                builder.AddApi(typeof(AnotherHelloWorld.MultiTask.ApiFactory));
                builder.AddEndpoint("/mytest", typeof(AnotherHelloWorld.MultiTask.ApiFactory).FullName);
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
