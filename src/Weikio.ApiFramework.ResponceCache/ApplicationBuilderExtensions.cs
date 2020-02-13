using Microsoft.AspNetCore.Builder;

namespace Weikio.ApiFramework.ResponceCache
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiFrameworkResponseCaching(this IApplicationBuilder app)
        {
            app.UseMiddleware<ResponseCacheControlMiddleware>();

            return app;
        }
    }
}
