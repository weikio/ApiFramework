//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using Weikio.ApiFramework.AspNetCore;
//using Weikio.ApiFramework.Core.Extensions;
//
//namespace ApiFramework.IntegrationTests
//{
//    public class CustomWebApplicationFactory<TStartup>
//        : WebApplicationFactory<TStartup> where TStartup : class
//    {
//        protected override void ConfigureWebHost(IWebHostBuilder builder)
//        {
//            builder.ConfigureServices(services =>
//            {
//                services.AddRouting();
//                services.AddMvc();
//
//                services.AddApiFramework(x =>
//                    {
//                        x.AutoResolveApis = false;
//                        x.AutoResolveEndpoints = false;
//                    })
//                    .AddApi(typeof(HelloWorldApi))
//                    .AddEndpoint("/test", "HelloWorldApi");
//            });
//            
//            builder.Configure()
//        }
//    }
//    
//    public class HelloWorldApi
//    {
//        public string SayHello()
//        {
//            return "Hello Api Framework!";
//        }
//    }
//}
