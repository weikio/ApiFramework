using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.Admin;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.ApiFramework.Plugins.Broken;
using Weikio.ApiFramework.Plugins.HealthCheck;
using Weikio.ApiFramework.Plugins.HelloWorld;
using Weikio.ApiFramework.Plugins.JsonNetNew;
using Weikio.ApiFramework.Plugins.JsonNetOld;

namespace Weikio.ApiFramework.Samples.Admin
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

            var mvcBuilder = services.AddMvc(options =>
                {
                    options.Filters.Add(new ApiConfigurationActionFilter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("custom", policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireAssertion(context =>
                    {
                        var res = false;

                        return res;
                    });
                });
            });

            services.AddApiFramework(options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.AutoResolveApis = false;
                })
                .AddApi(typeof(ApiFactory))
                .AddApi(typeof(HelloWorldApi))
                .AddApi(typeof(NewJsonApi))
                .AddApi(typeof(OldJsonApi))
                .AddApi(typeof(Weikio.ApiFramework.Plugins.DynamicHelloWorld.ApiFactory))
                .AddApi(typeof(SometimesWorkingApi))
                .AddEndpoint("/sometimesworks", "HealthCheck")
                .AddEndpoint("/notworking", "Broken", healthCheck: new CustomHealthCheck())
                .AddEndpoint("/working", "HelloWorld", new { HelloString = "Fast Hellou!!!" })
                .AddEndpoint("/working2", "HelloWorld", new { HelloString = "Another configuration Hellou!!!" })
                .AddAdmin(options =>
                {
                    options.EndpointApiPolicy = "custom";
                    options.EndpointAdminRouteRoot = "myadmin/here/itis";
                });

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework";
                document.DocumentName = "api";
                // document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor("api_framework_endpoint"));
            });
            
            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework";
                document.DocumentName = "external";
                // document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor("external"));
            });

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework";
                document.ApiGroupNames = new[] { "api_framework_admin" };
                document.DocumentName = "api_admin";
            });

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Everything";
                document.DocumentName = "all";
            });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();
            app.UseResponseCaching();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapHealthChecks("/myhealth", new HealthCheckOptions { Predicate = check => check.Tags.Contains("api_framework_endpoint") });

                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
