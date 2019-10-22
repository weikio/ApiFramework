using Weikio.ApiFramework.Samples.DependencyConflict;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace FunctionFramework.Samples.ReferenceConflict
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
                .UseStartup<Startup>();
        }
    }
}