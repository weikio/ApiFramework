using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
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

        public static Endpoint GetEndpointMetadata(this ControllerActionDescriptor controllerActionDescriptor)
        {
            var result = controllerActionDescriptor.EndpointMetadata?.OfType<Endpoint>().FirstOrDefault();

            return result;
        }
    }
}
