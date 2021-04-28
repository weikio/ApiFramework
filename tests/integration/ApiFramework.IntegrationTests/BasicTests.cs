using System;
using System.Net;
using System.Threading.Tasks;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class BasicTests : ApiFrameworkTestBase
    {
        public BasicTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanGetApi()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldApi");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello Api Framework!", result);
        }

        [Fact]
        public async Task CanGetApiWithMultipleFunctions()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldMultipleApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldMultipleApi");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest/sayhello");
            var secondResult = await server.GetStringAsync("/api/mytest/sayanother");

            // Assert
            Assert.Equal("Hello Api Framework!", result);
            Assert.Equal("Hello Another!", secondResult);
        }

        [Fact]
        public async Task CanGetApiWithParameter()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldParameterApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldParameterApi");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest?text=sometext");

            // Assert
            Assert.Equal("sometext", result);
        }

        [Fact]
        public async Task CanPostApi()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldPostApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldPostApi");
            });

            // Act 
            var response = await server.PostAsync("/api/mytest", null);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task CanPostApiWithParameters()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldParameterPostApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldParameterPostApi");
            });

            // Act 
            var result = await server.PostJsonAsync<CreatedResult>("/api/mytest", new CreatedDto { FirstName = "hello", Age = 50 });

            // Assert
            Assert.Equal("hello", result.FirstName);
            Assert.Equal(50, result.Age);
        }

        [Fact]
        public async Task CanChangeApiBasePath()
        {
            var server = Init(builder =>
                {
                    builder.AddApi(typeof(HelloWorldApi));
                    builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldApi");
                },
                options =>
                {
                    options.ApiAddressBase = "/changed";
                });

            // Act 
            var result = await server.GetStringAsync("/changed/mytest");

            // Assert
            Assert.Equal("Hello Api Framework!", result);
        }

        [Fact]
        public async Task MissingApiReturns404()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldApi");
            });

            // Act 
            var result = await server.GetAsync("/api/notexists");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }
        
        [Fact]
        public async Task MethodNamesAreRemovedByDefault()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldApi");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello Api Framework!", result);
        }
        
        [Fact]
        public async Task CanConfigureMethodNameRemoval()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldApi");
            }, options =>
            {
                options.AutoTidyUrls = AutoTidyUrlModeEnum.Disabled;
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest/HelloWorld/SayHello");

            // Assert
            Assert.Equal("Hello Api Framework!", result);
        }

        [Fact]
        public async Task CanConfigureAutoTidyUrlOnApiLevel()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                builder.AddApi(typeof(AnotherHelloWorldApi));
                builder.AddEndpoint("/mytest", "HelloWorld.HelloWorldApi");
                builder.AddEndpoint("/another", "HelloWorld.AnotherHelloWorldApi");

                builder.Services.Configure<AutoTidyUrlAPIOverrides>("HelloWorld.HelloWorldApi", options =>
                {
                    options.AutoTidyUrls = AutoTidyUrlModeEnum.Disabled;
                });
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest/HelloWorld/SayHello");
            var anotherResult = await server.GetStringAsync("/api/another");

            // Assert
            Assert.Equal("Hello Api Framework!", result);
            Assert.Equal("Another Hello Api Framework!", anotherResult);
        }
    }
}
