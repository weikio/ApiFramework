using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Extensions;

namespace Weikio.ApiFramework.Samples.JsonConfiguration
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
            services.AddMvc(options =>
                {
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddApiFramework(options => options.AutoResolveApis = true);

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework All Apis";
                document.DocumentName = "api";
                document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
            });

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework Internal APIs";
                document.DocumentName = "Internal";
                document.ApiGroupNames = new[] { "internal", "api_framework_endpoint" };
                document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
            });
            
            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework External APIs";
                document.DocumentName = "External";
                document.ApiGroupNames = new[] { "external" };
                document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
            });

            //
            // services.AddOpenApiDocument(document =>
            // {
            //     document.Title = "Api Framework";
            //     document.DocumentName = "external";
            //     document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor("external"));
            //     // document.SchemaProcessors.Add(new MySchemaProcessor());
            // });
            //
            // services.AddOpenApiDocument(document =>
            // {
            //     document.Title = "Api Framework";
            //     document.DocumentName = "internal";
            //     // document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor("internal"));
            // });
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
            });
        }
    }

    public class Customer
    {
        public string Firstname { get; set; }
    }

    public class CustomersApi
    {
        public Customer Get()
        {
            return new Customer() { Firstname = "Api-FF" };
        }
    }
}
