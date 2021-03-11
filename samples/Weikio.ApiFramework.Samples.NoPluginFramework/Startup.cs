using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.Infrastructure;

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
            services.AddControllers();
            
            services.AddApiFrameworkCore(options =>
                {
                    options.ApiCatalogs.Add(new TypeApiCatalog(typeof(HelloWorldWithoutPlugins)));
                    options.AutoResolveEndpoints = false;
                    options.ApiAddressBase = "/myapi";
                    options.EndpointHttpVerbResolver = new CustomHttpVerbResolver();
                })
                .AddEndpoint("/test", typeof(HelloWorldWithoutPlugins).FullName);

            services.AddOpenApiDocument(document => { document.Title = "Api Framework"; });
        }
        
        public class CustomHttpVerbResolver :IEndpointHttpVerbResolver
        {
            public string GetHttpVerb(ActionModel action)
            {
                return "POST";
            }
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
