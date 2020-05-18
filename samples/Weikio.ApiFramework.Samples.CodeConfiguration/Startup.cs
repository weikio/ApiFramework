using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Plugins.Soap;

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


            var apiFrameworkBuilder = services.AddApiFramework(options =>
            {
                options.AutoResolveApis = false;
                options.AutoResolveEndpoints = false;
            });

            services.AddSoapApi("/soaptest", "http://localhost:54533/Service1.svc");
            
                // }).AddApi(typeof(ApiFactory))
                // .AddEndpoint("/soaptest", "Weikio.ApiFramework.Plugins.Soap.ApiFactory",
                //     configuration: new
                //     {
                //         soapOptions = new SoapOptions()
                //         {
                //             WsdlLocation = "http://localhost:54533/Service1.svc"
                //         }
                //     });

//             services.AddApiFramework(options =>
//                 {
//                     options.AutoResolveEndpoints = false;
//                     options.ApiAddressBase = "/myapi";
//                     options.AutoResolveApis = false;
//
// //
// //                options.Endpoints = new List<(string Route, string FunctionAssemblyName, object Configuration)>()
// //                {
// //                    ("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"}),
// //                    ("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"}),
// //                };
//                 })
//                 
//                 // .AddEndpoint("/withhealth", "Weikio.ApiFramework.Plugins.HealthCheck");
//
// //                .AddEndpoint("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"})
// //                .AddEndpoint("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"});
//                 ;
            
//             services.AddApiFramework(options =>
//                 {
//                     options.AutoResolveEndpoints = false;
//                     options.ApiAddressBase = "/myapi";
//                     options.AutoResolveApis = false;
//
// //
// //                options.Endpoints = new List<(string Route, string FunctionAssemblyName, object Configuration)>()
// //                {
// //                    ("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"}),
// //                    ("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"}),
// //                };
//                 })
//                 
//                 // .AddEndpoint("/withhealth", "Weikio.ApiFramework.Plugins.HealthCheck");
//
// //                .AddEndpoint("/test", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "Hey there from first configuration"})
// //                .AddEndpoint("/otherEndpoint", "Weikio.ApiFramework.Plugins.HelloWorld", new {HelloString = "This is the second configuration"});
//             ;

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

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapHealthChecks("/myhealth",
                        new HealthCheckOptions { Predicate = check => check.Tags.Contains("api_framework_endpoint") });

                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
