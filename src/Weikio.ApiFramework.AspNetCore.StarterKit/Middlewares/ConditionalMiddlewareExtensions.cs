using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Weikio.ApiFramework.AspNetCore.StarterKit.Middlewares
{
    public static class ConditionalMiddlewareExtensions
    {
        // Add ConditionalMiddlware that runs just before the middleware given by "beforeMiddleware"
        public static IServiceCollection AddConditionalMiddleware(this IServiceCollection services)
        {
            return services.AddTransient<IStartupFilter>(_ => new ConditionalMiddlewareStartupFilter());
        }
    }
}