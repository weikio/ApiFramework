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
                    options.Filters.Add(new FunctionConfigurationActionFilter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddFunctionFramework(mvcBuilder, options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.AutoResolveFunctions = false;
                })
                .AddFunction(typeof(Weikio.ApiFramework.Plugins.Broken.FunctionFactory))
                .AddFunction(typeof(Weikio.ApiFramework.Plugins.HelloWorld.HelloWorldFunction))
                .AddFunction(typeof(Weikio.ApiFramework.Plugins.JsonNetNew.NewJsonFunction))
                .AddFunction(typeof(Weikio.ApiFramework.Plugins.JsonNetOld.OldJsonFunction))
                .AddFunction(typeof(FunctionFactory))
                .AddFunction(typeof(FunctionFactory))
                .AddFunction(typeof(Weikio.ApiFramework.Plugins.HealthCheck.SometimesWorkingFunction))
                .AddEndpoint("/sometimesworks", "HealthCheck")
                .AddEndpoint("/notworking", "Broken", healthCheck: new CustomHealthCheck())
                .AddEndpoint("/working", "HelloWorld", new {HelloString = "Fast Hellou!!!"});

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Function Framework";
                document.ApiGroupNames = new[] {"function_framework_function"};
                document.DocumentName = "func";
            });

            services.AddOpenApiDocument(document =>
            {
                document.Title = "Function Framework";
                document.ApiGroupNames = new[] {"function_framework_admin"};
                document.DocumentName = "func_admin";
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
            app.UseFunctionFrameworkResponseCaching();

            app.UseOpenApi();
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