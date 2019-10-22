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
            services.AddResponseCaching();
            services.AddRouting();

            var mvcBuilder = services.AddMvc(options => { })
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddFunctionFramework(mvcBuilder, options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.FunctionAddressBase = "/api";
                    options.AutoResolveFunctions = false;
                })
                .AddFunction(
                    @"C:\dev\projects\Weik.io\src\FunctionFramework\src\Plugins\Weikio.ApiFramework.Plugins.Logger\bin\Debug\netstandard2.0\Weikio.ApiFramework.Plugins.Logger.dll")
                .AddFunction(
                    @"C:\dev\projects\Weik.io\src\FunctionFramework\src\Plugins\Weikio.ApiFramework.Plugins.OldLogger\bin\Debug\netstandard2.0\Weikio.ApiFramework.Plugins.OldLogger.dll")
                .AddFunction(typeof(HostLoggerFunction))
                .AddEndpoint("/new", "Weikio.ApiFramework.Plugins.Logger")
                .AddEndpoint("/old", "Weikio.ApiFramework.Plugins.OldLogger")
                .AddEndpoint("/host", "DependencyConflict");

//                .AddEndpoint("/old", "Weikio.ApiFramework.Plugins.JsonNetOld", new {HelloString = "This is the second configuration"});

            services.AddSwaggerDocument(document => { document.Title = "Function Framework"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            app.UseRouting();

            app.UseResponseCaching();
            app.UseFunctionFrameworkResponseCaching();

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

    public class HostLoggerFunction
    {
        private readonly ILogger<HostLoggerFunction> _logger;

        public HostLoggerFunction(ILogger<HostLoggerFunction> logger)
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