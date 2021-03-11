using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSwag.Generation.Processors;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.AspNetCore.NSwag;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Samples.PluginLibrary;

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

            var builder = services.AddApiFramework(options =>
            {
                options.AutoTidyUrls = AutoTidyUrlModeEnum.Disabled;
            });
            
            builder.AddApi(typeof(HelloWorldApi));
            builder.AddEndpoint("/first", "Weikio.ApiFramework.Samples.PluginLibrary.HelloWorldApi");
            builder.AddEndpoint("/second", "Weikio.ApiFramework.Samples.PluginLibrary.HelloWorldApi");
            
            // builder.AddApi(typeof(SampleController));
            // builder.AddEndpoint("/mytest", "Weikio.ApiFramework.Samples.PluginLibrary.SampleController");
                
//            services.AddFunctionFramework(Configuration, mvcBuilder, options =>
//            {
//                options.AutoResolveFunctions = false;
//                options.AutoResolveEndpoints = false;
//            });

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

            services.AddOpenApiDocument(document => { document.Title = "Api Framework"; });
            services.AddTransient<IDocumentProcessor, OpenApiExtenderDocumentProcessor>();
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
