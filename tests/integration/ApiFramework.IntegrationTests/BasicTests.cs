using System.Net;
using System.Threading.Tasks;
using CodeConfiguration;
using HelloWorld;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;
using Xunit;

namespace ApiFramework.IntegrationTests
{
    public class BasicTests : ApiFrameworkTestBase
    {
        public BasicTests(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CanGetApi()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldApi));
                builder.AddEndpoint("/mytest", "HelloWorld");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello Api Framework!", result);
        }

        [Fact]
        public async Task CanGetApiWithParameter()
        {
            var server = Init(builder =>
            {
                builder.AddApi(typeof(HelloWorldParameterApi));
                builder.AddEndpoint("/mytest", "HelloWorld");
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
                builder.AddEndpoint("/mytest", "HelloWorld");
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
                builder.AddEndpoint("/mytest", "HelloWorld");
            });

            // Act 
            var result = await server.PostJsonAsync<CreatedResult>("/api/mytest", new CreatedDto {FirstName = "hello", Age = 50});

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
                    builder.AddEndpoint("/mytest", "HelloWorld");
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
                builder.AddEndpoint("/mytest", "HelloWorld");
            });

            // Act 
            var result = await server.GetAsync("/api/notexists");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}
