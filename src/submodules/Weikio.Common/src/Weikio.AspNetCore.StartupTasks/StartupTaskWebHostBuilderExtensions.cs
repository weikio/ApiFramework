using Microsoft.AspNetCore.Hosting;

namespace Weikio.AspNetCore.StartupTasks
{
//    public static class StartupTaskServiceCollectionExtensions
//    {
//        public static void AddStartupTasks(this IServiceCollection services)
//        {
//            
//        }
//    }
    public static class StartupTaskWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseStartupTasks(this IWebHostBuilder webHostBuilder,
            bool autoRegisterStartupTasks = true, bool enableHealthCheck = true)
        {
            webHostBuilder.ConfigureServices((context, services) =>
            {
                var healtcheckparams = new StartupTasksHealthCheckParameters { IsEnabled = enableHealthCheck };

                services.AddStartupTasks(autoRegisterStartupTasks, healtcheckparams);
            });

            return webHostBuilder;
        }
    }
}
