using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.Generation.Processors.Security;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.SDK;

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

            // services.AddTransient<IPluginCatalog>(s =>
            // {
            //     var assemblyPath = typeof(ProcountorOptions).Assembly.Location;
            //     var assemblyCatalog = new AssemblyPluginCatalog(assemblyPath);
            //
            //     return assemblyCatalog;
            // });
            //
            // services.AddTransient<IPluginCatalog>(s =>
            // {
            //     var assemblyPath = typeof(MySqlOptions).Assembly.Location;
            //     var assemblyCatalog = new AssemblyPluginCatalog(assemblyPath);
            //
            //     return assemblyCatalog;
            // });
            //
            // services.AddOpenApiDocument(document =>
            // {
            //     document.Title = "Api Framework All Apis";
            //     document.DocumentName = "api";
            //     document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
            // });
            //
            // services.AddOpenApiDocument(document =>
            // {
            //     document.Title = "Api Framework Internal APIs";
            //     document.DocumentName = "Internal";
            //     document.ApiGroupNames = new[] { "internal", "api_framework_endpoint" };
            //     document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
            // });
            //
            // services.AddOpenApiDocument(document =>
            // {
            //     document.Title = "Api Framework External APIs";
            //     document.DocumentName = "External";
            //     document.ApiGroupNames = new[] { "external" };
            //     document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
            // });

            var endpointConfigurations = Configuration.GetSection("Documents").GetChildren();

            foreach (var endpointConfiguration in endpointConfigurations)
            {
                var name = endpointConfiguration["Name"];
                var routePrefix = '/' + endpointConfiguration["RoutePrefix"].Trim('/');
                var endpoints = endpointConfiguration.GetSection("Endpoints").GetChildren();

                foreach (var endpoint in endpoints)
                {
                    services.AddTransient(sp =>
                    {
                        var endpointRoute = routePrefix + '/' + endpoint["Endpoint"].Trim('/');
                        var api = endpoint["Api"];
                        var endpointConfig = endpoint.GetSection("Configuration").ToDictionary();

                        var defaultApiConfigSection = Configuration.GetSection(api);

                        if (defaultApiConfigSection != null)
                        {
                            var defaultApiConfig = defaultApiConfigSection.ToDictionary();

                            if (defaultApiConfig != null)
                            {
                                foreach (var keyValue in defaultApiConfig)
                                {
                                    endpointConfig.TryAdd(keyValue.Key, keyValue.Value);
                                }
                            }
                        }

                        var result = new EndpointDefinition(endpointRoute, api, endpointConfig, null, name.ToLowerInvariant());

                        return result;
                    });
                }

                services.AddOpenApiDocument(settings =>
                {
                    settings.Title = $"{name} - Eqvitia Weik.io";
                    settings.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
                    settings.ApiGroupNames = new[] { name.ToLowerInvariant() };
                    settings.DocumentName = name;

                    settings.AddSecurity("Basic", Enumerable.Empty<string>(),
                        new OpenApiSecurityScheme
                        {
                            Type = OpenApiSecuritySchemeType.Basic,
                            Name = "Authorization",
                            In = OpenApiSecurityApiKeyLocation.Header,
                            Description = "Provides Basic Authentication"
                        });

                    settings.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Basic"));
                });
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
