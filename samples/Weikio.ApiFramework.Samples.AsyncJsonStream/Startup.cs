using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.AspNetCore.StarterKit;
using Weikio.ApiFramework.Core.AsyncStream;
using Weikio.ApiFramework.Plugins.MySql;
using Weikio.ApiFramework.Plugins.MySql.Configuration;

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
            
            services.AddApiFrameworkWithAdmin()
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
