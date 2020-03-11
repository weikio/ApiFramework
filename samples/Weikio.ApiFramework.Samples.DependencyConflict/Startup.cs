using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;

namespace Weikio.ApiFramework.Samples.DependencyConflict
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddApiFramework(options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.ApiAddressBase = "/api";
                    options.AutoResolveApis = false;
                })
                .AddApi(
                    @"..\..\src\Plugins\Weikio.ApiFramework.Plugins.Logger\bin\Debug\netstandard2.0\Weikio.ApiFramework.Plugins.Logger.dll")
                .AddApi(
                    @"..\..\src\Plugins\Weikio.ApiFramework.Plugins.OldLogger\bin\Debug\netstandard2.0\Weikio.ApiFramework.Plugins.OldLogger.dll")
                .AddApi(typeof(HostLoggerApi))
                .AddEndpoint("/new", "Weikio.ApiFramework.Plugins.Logger")
                .AddEndpoint("/old", "Weikio.ApiFramework.Plugins.OldLogger")
                .AddEndpoint("/host", "Weikio.ApiFramework.Samples.DependencyConflict.HostLoggerApi");

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

            app.UseOpenApi();
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
}
