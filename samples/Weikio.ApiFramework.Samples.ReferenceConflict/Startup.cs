using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;

namespace Weikio.ApiFramework.Samples.ReferenceConflict
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

            services.AddApiFramework(mvcBuilder, options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.ApiAddressBase = "/api";
                    options.AutoResolveApis = false;
                })
                .AddApi(
                    @"C:\dev\projects\Weikio\src\FunctionFramework\src\Plugins\Weikio.ApiFramework.Plugins.JsonNetOld\bin\Debug\netstandard2.0\Weikio.ApiFramework.Plugins.JsonNetOld.dll")
                .AddApi(
                    @"C:\dev\projects\Weikio\src\FunctionFramework\src\Plugins\Weikio.ApiFramework.Plugins.JsonNetNew\bin\Debug\netstandard2.0\Weikio.ApiFramework.Plugins.JsonNetNew.dll")
                .AddEndpoint("/new", "Weikio.ApiFramework.Plugins.JsonNetNew",
                    new {HelloString = "Hey there from first configuration"})
                .AddEndpoint("/old", "Weikio.ApiFramework.Plugins.JsonNetOld",
                    new {HelloString = "This is the second configuration"});

            services.AddSwaggerDocument(document => { document.Title = "Api Framework"; });
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
            app.UseApiFrameworkResponseCaching();

            app.UseSwagger();
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