using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.ApiFramework.AspNetCore.StarterKit;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Samples.JsonConfiguration
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            services.AddApiFrameworkStarterKit(options =>
            {
                options.ApiVersionMatchingBehaviour = ApiVersionMatchingBehaviour.Lowest;
            });

            services.Configure<PluginFrameworkApiProviderOptions>(options =>
            {
                options.GetNugetApiInstallRoot = (package, version, serviceProvider) =>
                {
                    var result = Path.Combine(AppContext.BaseDirectory, "plugins", "apis", $"{package}.{version}");

                    return result;
                };
            });

            services.AddTransient<IEndpointStatusObserverFactory, CustomEndpointStatusObserverFactory>();
        }

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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
    
    public class CustomEndpointStatusObserver : IStatusObserver<EndpointStatusEnum>
    {
        private readonly Endpoint _endpoint;

        public CustomEndpointStatusObserver(Endpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public void Observe(StatusLog<EndpointStatusEnum> newStatus)
        {
            Debug.WriteLine(newStatus.ToString());
        }
    }
    
    public class CustomEndpointStatusObserverFactory : IEndpointStatusObserverFactory
    {
        public IStatusObserver<EndpointStatusEnum> Create(IServiceProvider serviceProvider, Endpoint endpoint)
        {
            return new CustomEndpointStatusObserver(endpoint);
        }
    }
}
