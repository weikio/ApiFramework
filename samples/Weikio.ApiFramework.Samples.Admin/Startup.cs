using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.AspNetCore.StarterKit;

namespace Weikio.ApiFramework.Samples.Admin
{
    public class ControllerAc : IControllerActivator
    {
        private readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache =
            new ConcurrentDictionary<Type, ObjectFactory>();
        
        public object Create(ControllerContext controllerContext)
        {
            var isApi = controllerContext.HttpContext.GetEndpointMetadata();

            IServiceProvider serviceProvider;
            if (isApi.Metadata.FirstOrDefault() != null)
            {
                var coll = (IServiceCollection) isApi.Metadata.First();
                serviceProvider = coll.BuildServiceProvider(new ServiceProviderOptions() { });
            }
            else
            {
                serviceProvider = controllerContext.HttpContext.RequestServices;
            }
            
            var controllerTypeInfo = controllerContext.ActionDescriptor.ControllerTypeInfo;

            var factory = _typeActivatorCache.GetOrAdd(controllerTypeInfo.AsType(), type => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes));
            var result = factory(serviceProvider, arguments: null);

            return result;
        }

        public void Release(ControllerContext context, object controller)
        {
            if (controller is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
    
    

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
            services.TryAddTransient<IControllerActivator, ControllerAc>();
            services.AddControllers();
            services.AddSingleton<IServiceCollection>(services);
            
            services.AddApiFrameworkStarterKit(options =>
                {
                    options.AutoResolveApis = false;
                })
                .AddApi<HelloWorldApiFactory>("/hello");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
