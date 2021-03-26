using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.AspNetCore.StarterKit;
using Weikio.ApiFramework.Core.AsyncStream;

namespace Weikio.ApiFramework.Samples.AsyncJsonStream
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.Configure<AsyncStreamJsonOptions>(options =>
            {
                options.IsEnabled = false;
            });
            
            services.Configure<AsyncStreamJsonOptions>("/myweather", options =>
            {
                options.IsEnabled = true;
                options.BufferSizeThresholdInKB = 1024;
            });
            
            services.AddApiFrameworkStarterKit()
                .AddApi<WeatherApi>("/myweather", new WeatherConfiguration());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
