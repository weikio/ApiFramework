using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;

namespace Weikio.ApiFramework.Samples.DependencyConflict
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
            services.AddRouting();

            var mvcBuilder = services.AddMvc(options => { })
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddApiFramework(options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.ApiAddressBase = "/api";
                    options.AutoResolveApis = false;
                })
                .AddApi(
                    @"C:\dev\projects\Weik.io\src\FunctionFramework\src\Plugins\Weikio.ApiFramework.Plugins.Logger\bin\Debug\netstandard2.0\Weikio.ApiFramework.Plugins.Logger.dll")
                .AddApi(
                    @"C:\dev\projects\Weik.io\src\FunctionFramework\src\Plugins\Weikio.ApiFramework.Plugins.OldLogger\bin\Debug\netstandard2.0\Weikio.ApiFramework.Plugins.OldLogger.dll")
                .AddApi(typeof(HostLoggerApi))
                .AddEndpoint("/new", "Weikio.ApiFramework.Plugins.Logger")
                .AddEndpoint("/old", "Weikio.ApiFramework.Plugins.OldLogger")
                .AddEndpoint("/host", "DependencyConflict");

//                .AddEndpoint("/old", "Weikio.ApiFramework.Plugins.JsonNetOld", new {HelloString = "This is the second configuration"});

            services.AddSwaggerDocument(document => { document.Title = "Api Framework"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapGet("/logger", async context =>
                {
                    var logger = context.RequestServices.GetService<ILogger<Startup>>();

                    var assembly = logger.GetType().Assembly;
                    var location = assembly.Location;

                    await context.Response.WriteAsync("Logger location: " + location);
                });
            });
        }
    }

    public class HostLoggerApi
    {
        private readonly ILogger<HostLoggerApi> _logger;

        public HostLoggerApi(ILogger<HostLoggerApi> logger)
        {
            _logger = logger;
        }

        public string Log()
        {
            _logger.LogInformation("Running host logger");

            var assembly = _logger.GetType().Assembly;
            var location = assembly.Location;

            var versionInfo = FileVersionInfo.GetVersionInfo(location);
            return location + " " + versionInfo.ToString();
        }
    }
}
