using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public sealed class ApiConfigurationActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor))
            {
                return;
            }

            var configurationSetter = controllerActionDescriptor.EndpointMetadata.OfType<Action<object>>().FirstOrDefault();

            configurationSetter?.Invoke(context.Controller);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
