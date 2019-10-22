using Microsoft.AspNetCore.Builder;
using Weikio.ApiFramework.Core.Caching;

namespace Weikio.ApiFramework.Core.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFunctionFrameworkResponseCaching(this IApplicationBuilder app)
        {
            app.UseMiddleware<ResponseCacheControlMiddleware>();

            return app;
        }
    }
}
