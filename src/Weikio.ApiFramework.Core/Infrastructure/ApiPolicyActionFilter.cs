using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.AspNetCore;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public sealed class ApiPolicyActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<ApiPolicyActionFilter> _logger;

        public ApiPolicyActionFilter(IAuthorizationService authorizationService, ILogger<ApiPolicyActionFilter> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public int Order { get; } = -1;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
            {
                return;
            }

            var endpointMetadata = controllerActionDescriptor.GetEndpointMetadata();

            if (endpointMetadata == null)
            {
                await next();

                return;
            }

            if (string.IsNullOrWhiteSpace(endpointMetadata.Definition.Policy))
            {
                _logger.LogTrace("No authorization policy set for endpoint {Endpoint}", endpointMetadata);

                await next();

                return;
            }

            _logger.LogTrace("Authorizing user {User} to endpoint {Endpoint} using policy {Policy}", context.HttpContext.User, endpointMetadata,
                endpointMetadata.Definition.Policy);

            var result = await _authorizationService.AuthorizeAsync(context.HttpContext.User, endpointMetadata.Definition.Policy);

            if (result.Succeeded)
            {
                _logger.LogTrace("Authorized user {User} to endpoint {Endpoint} using policy {Policy}", context.HttpContext.User, endpointMetadata,
                    endpointMetadata.Definition.Policy);
                await next();

                return;
            }

            _logger.LogTrace("Denied access for user {User} to endpoint {Endpoint} using policy {Policy}", context.HttpContext.User, endpointMetadata,
                endpointMetadata.Definition.Policy);

            await context.HttpContext.ForbidAsync();
        }
    }
}
