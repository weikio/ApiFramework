using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.ApiFramework.Plugins.DynamicHelloWorld;

namespace RuntimeConfiguration
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

            services.AddApiFramework(mvcBuilder, options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.AutoResolveApis = false;
                })
                .AddApi(typeof(Weikio.ApiFramework.Plugins.Broken.ApiFactory))
                .AddApi(typeof(Weikio.ApiFramework.Plugins.HelloWorld.HelloWorldApi))
                .AddApi(typeof(Weikio.ApiFramework.Plugins.JsonNetNew.NewJsonApi))
                .AddApi(typeof(Weikio.ApiFramework.Plugins.JsonNetOld.OldJsonApi))
                .AddApi(typeof(ApiFactory))
                .AddApi(typeof(Weikio.ApiFramework.Plugins.HealthCheck.SometimesWorkingApi))
                .AddEndpoint("/sometimesworks", "HealthCheck")
                .AddEndpoint("/notworking", "Broken", healthCheck: new CustomHealthCheck())
                .AddEndpoint("/working", "HelloWorld", new {HelloString = "Fast Hellou!!!"});

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework";
                document.ApiGroupNames = new[] {"api_framework_function"};
                document.DocumentName = "api";
            });

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework";
                document.ApiGroupNames = new[] {"api_framework_admin"};
                document.DocumentName = "api_admin";
            });

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Everything";
                document.DocumentName = "all";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            app.UseRouting();
            app.UseResponseCaching();
            app.UseApiFrameworkResponseCaching();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapHealthChecks("/myhealth",
                        new HealthCheckOptions()
                            {Predicate = (check) => check.Tags.Contains("api_framework_endpoint")});

                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class CustomHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new HealthCheckResult(HealthStatus.Degraded, "External");

            return Task.FromResult(result);
        }
    }
}