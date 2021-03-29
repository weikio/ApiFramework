using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeConfiguration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.PluginFramework.Catalogs.Delegates;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class DelegateApiTests : ApiFrameworkTestBase
    {
        public DelegateApiTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanCreateApiFromDelegate()
        {
            var server = Init(builder =>
            {
                builder.AddApi(new Func<string>(() => "Hello from Delegate Api"), "HelloDelegate");
                
                builder.AddEndpoint("/mytest", "HelloDelegate");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello from Delegate Api", result);
        }
        
        [Fact]
        public async Task CanCreateApiWithParametersFromDelegate()
        {
            var server = Init(builder =>
            {
                builder.AddApi(new Func<int, int, string>((x, y) => (x+y).ToString()), "HelloDelegate");
                
                builder.AddEndpoint("/mytest", "HelloDelegate");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest?x=10&y=5");

            // Assert
            Assert.Equal("15", result);
        }
        
        [Fact]
        public async Task CanAutoResolveServices()
        {
            var server = Init(builder =>
            {
                builder.Services.AddSingleton<PrivateService>();
                builder.AddApi(new Func<PrivateService, int, int, string>((service, x, y) => (service.Get() + x + y).ToString()), "HelloDelegate");
                
                builder.AddEndpoint("/mytest", "HelloDelegate");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest?x=10&y=5");

            // Assert
            Assert.Equal("18", result);
        }
        
        [Fact]
        public async Task CanInjectServicesByRules()
        {
            var server = Init(builder =>
            {
                builder.AddApi(new Func<PrivateService, int, int, string>((service, x, y) => (service.Get() + x+y).ToString()), "HelloDelegate");
                
                builder.AddEndpoint("/mytest", "HelloDelegate");
                
                builder.Services.AddSingleton<PrivateService>();
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest?x=10&y=5");

            // Assert
            Assert.Equal("18", result);
        }
        
        [Fact]
        public async Task CanCreateMultipleApi()
        {
            var server = Init(builder =>
            {
                builder.AddApi(new Func<string>(() => "Hello from Delegate Api"), "HelloDelegate");
                builder.AddEndpoint("/mytest", "HelloDelegate");

                builder.AddApi(new Func<string>(() => "Hello from Another Api"), "AnotherDelegate");
                builder.AddEndpoint("/anothertest", "AnotherDelegate");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");
            var result2 = await server.GetStringAsync("/api/anothertest");

            // Assert
            Assert.Equal("Hello from Delegate Api", result);
            Assert.Equal("Hello from Another Api", result2);
        }
        
        [Fact]
        public async Task CanCreateApiWithConfiguration()
        {
            var server = Init(builder =>
            {
                builder.AddApi(new Func<MyDelegateConfiguration, string>(configuration => configuration.Z.ToString()), "HelloDelegate", catalogOptions: new DelegatePluginCatalogOptions()
                {
                    ConversionRules = new List<DelegateConversionRule>()
                    {
                        new DelegateConversionRule(info => info.Name == "configuration", nfo => new ParameterConversion() { ToPublicProperty = true }),
                    }
                });
                
                builder.AddEndpoint("/mytest", "HelloDelegate", new MyDelegateConfiguration()
                {
                    Z = 11
                });
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("11", result);
        }
        
        [Fact]
        public async Task CanCreateApiAndEndpointFromDelegate()
        {
            var server = Init(builder =>
            {
                builder.AddApi(new Func<string>(() => "Hello from Delegate Api"), route: "/mytest");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello from Delegate Api", result);
        }
    }
    
    public class PrivateService
    {
        public int Get()
        {
            return 3;
        }
    }

    public class MyDelegateConfiguration
    {
        public int Z { get; set; } = 1;
    }
}
