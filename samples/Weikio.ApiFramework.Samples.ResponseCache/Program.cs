using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Weikio.ApiFramework.Core.Extensions;

namespace Weikio.ApiFramework.Samples.ResponseCache
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .AddApiFrameworkJsonConfigurationFile()
                .UseStartup<Startup>();
        }
    }
}