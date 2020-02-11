using Microsoft.AspNetCore.Builder;
using Weikio.ApiFramework.Core.Configuration;

namespace Weikio.ApiFramework.Extensions.ResponceCache
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
