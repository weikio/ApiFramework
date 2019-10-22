using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;

namespace Weikio.ApiFramework.Samples.CodeConfiguration
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

//            services.AddFunctionFramework(Configuration, mvcBuilder, options =>
//            {
//                options.AutoResolveFunctions = false;
//                options.AutoResolveEndpoints = false;
//            });

            services.AddFunctionFramework(mvcBuilder, options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.FunctionAddressBase = "/myapi";
//
//                options.Endpoints = new List<(string Route, string FunctionAssemblyName, object Configuration)>()
//                {
//                    ("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"}),
//                    ("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"}),
//                };
                })
                .AddEndpoint("/withhealth", "Weikio.ApiFramework.Plugins.HealthCheck");
//                .AddEndpoint("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"})
//                .AddEndpoint("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"});
            ;

//            services.AddFunctionFrameworkCore(Configuration, mvcBuilder, options =>
//            {
//                options.AutoResolveEndpoints = false;
//                options.FunctionAddressBase = "/api";
//
//                options.Endpoints = new List<(string Route, string FunctionAssemblyName, object Configuration)>()
//                {
//                    ("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"}),
//                    ("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"}),
//                };
//            }).AddPluginFramework(options =>
//            {
//                options.AutoResolveFunctions = false;
//                options.FunctionAssemblies = new List<string>() {typeof(Weikio.ApiFramework.Plugins.HelloWorld.HelloWorldFunction).Assembly.Location};
//            });

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
                endpoints
                    .MapHealthChecks("/myhealth",
                        new HealthCheckOptions()
                            {Predicate = (check) => check.Tags.Contains("function_framework_endpoint")});

                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}