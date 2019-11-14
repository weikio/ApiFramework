using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.HealthChecks;

namespace Weikio.ApiFramework.Samples.GeneratedApi
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

            services.AddApiFramework(options =>
            {
                options.AutoResolveEndpoints = false;

                options.Endpoints =
                    new List<(string Route, string ApiAssemblyName, object Configuration, IHealthCheck healthCheck)>
                    {
                        ("/dynamictest", "Weikio.ApiFramework.Plugins.DynamicHelloWorld", new
                        {
                            HelloString = "Hey there from first configuration",
                            postFix = "test!",
                            age = 20,
                            complex = new { Another = "Hey there", MyParameters = new List<string> { "first", "second" } }
                        }, new EmptyHealthCheck())
                    };
            });

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

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            {
                app.UseHsts();
            }

            app.UseRouting();

            app.UseResponseCaching();
            app.UseApiFrameworkResponseCaching();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
