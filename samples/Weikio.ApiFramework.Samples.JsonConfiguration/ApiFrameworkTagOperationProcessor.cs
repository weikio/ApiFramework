using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Samples.JsonConfiguration
{
    public class ApiFrameworkTagOperationProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            if (!(context is AspNetCoreOperationProcessorContext aspnetContext))
            {
                return true;
            }

            var tags = context.OperationDescription.Operation.Tags;
            var apiDescription = (Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription) typeof(AspNetCoreOperationProcessorContext).GetProperty("ApiDescription")?.GetValue(aspnetContext);

            if (apiDescription == null)
            {
                return true;
            }

            var endPoint = apiDescription.ActionDescriptor.EndpointMetadata?.OfType<Endpoint>().FirstOrDefault();

            if (endPoint != null)
            {
                tags.Clear();

                if (!string.IsNullOrWhiteSpace(endPoint.Name))
                {
                    tags.Add(endPoint.Name);
                }
                else
                {
                    tags.Add(endPoint.Route);
                }
            }
            else if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                if (!controllerActionDescriptor.ControllerTypeInfo.Assembly.FullName.Contains("Weikio.ApiFramework.Admin"))
                {
                    return true;
                }

                tags.Clear();
                tags.Add("Admin");
            }

            return true;
        }
    }
}
