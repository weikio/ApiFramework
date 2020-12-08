using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Weikio.ApiFramework.AspNetCore.StarterKit.Middlewares
{
    public static class RuntimeMiddlewareExtensions
    {
        public static IServiceCollection AddRuntimeMiddleware(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            services.Add(new ServiceDescriptor(typeof(RuntimeMiddlewareService), typeof(RuntimeMiddlewareService), lifetime));
            return services;
        }

        public static IApplicationBuilder UseRuntimeMiddleware(this IApplicationBuilder app)
        {
            var service = app.ApplicationServices.GetRequiredService<RuntimeMiddlewareService>();
            service.Use(app);
            
            return app;
        }
    }
}