using System.Linq;
using Microsoft.AspNetCore.Http;
using Endpoint = Weikio.ApiFramework.Abstractions.Endpoint;

namespace Weikio.ApiFramework.AspNetCore
{
    public static class HttpContextExtensions
    {
        public static Endpoint GetEndpointMetadata(this HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();

            var result = endpoint?.Metadata?.OfType<Endpoint>().FirstOrDefault();

            return result;
        }
    }
}
