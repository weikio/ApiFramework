using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Extensions;

namespace Weikio.ApiFramework.Samples.NoPluginFramework
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

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddApiFrameworkCore(options =>
                {
                    options.ApiProvider = new TypeApiProvider(typeof(HelloWorldWithoutPlugins));
                    options.AutoResolveEndpoints = false;
                    options.ApiAddressBase = "/myapi";
                })
                .AddEndpoint("/test", typeof(HelloWorldWithoutPlugins).FullName);

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
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
