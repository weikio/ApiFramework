using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ApiFramework.IntegrationTests.Infrastructure;
using CodeConfiguration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework;
using Weikio.PluginFramework.Catalogs.Roslyn;
using Xunit;
using Xunit.Abstractions;

namespace ApiFramework.IntegrationTests
{
    public class RoslynApiTests : ApiFrameworkTestBase
    {
        public RoslynApiTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }
        
        [Fact]
        public async Task CanCreateApiFromRoslynScript()
        {
            var script = "var x = \"Hello from Roslyn Api\"; return x;";
            
            var server = Init(builder =>
            {
                builder.AddApi(script, "HelloRoslyn");
                builder.AddEndpoint("/mytest", "HelloRoslyn");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("Hello from Roslyn Api", result);
        }
        
        [Fact]
        public async Task CanCreateApiFromRoslynCode()
        {
            var code = @"public class MyClass
                   {
                       public int RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           return a;
                       }
                   }";
            
            var server = Init(builder =>
            {
                builder.AddApi(code, "HelloRoslyn");
                builder.AddEndpoint("/mytest", "HelloRoslyn");
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("10", result);
        }

        [Fact]
        public async Task CanUseDi()
        {
            // We also create an another Roslyn Plugin Catalog to show how to create a plugin from a class. 
            // This catalog also uses dependency injection, external references and additional namespaces.
            var code = @"public class MyClass
                   {
                       private ExternalService _service;
                       public MyClass(ExternalService service)
                       {
                            _service = service;
                       } 

                       public string RunThings()
                       {
                            var result = JsonConvert.SerializeObject(15);
                            result += _service.DoWork();
                            return result; 
                       }
                   }";

            var options = new RoslynPluginCatalogOptions()
            {
                AdditionalReferences = new List<Assembly>() { typeof(Newtonsoft.Json.JsonConvert).Assembly, typeof(ExternalService).Assembly },
                AdditionalNamespaces = new List<string>() { "Newtonsoft.Json", "ApiFramework.IntegrationTests" }
            };
            
            var server = Init(builder =>
            {
                builder.AddApi(code, "HelloRoslyn", catalogOptions: options);
                builder.AddEndpoint("/mytest", "HelloRoslyn");

                builder.Services.AddSingleton<ExternalService>();
            });

            // Act 
            var result = await server.GetStringAsync("/api/mytest");

            // Assert
            Assert.Equal("15External service did some work", result);
        }
    }
    public class ExternalService
    {
        public string DoWork()
        {
            return "External service did some work";
        }
    }
}
