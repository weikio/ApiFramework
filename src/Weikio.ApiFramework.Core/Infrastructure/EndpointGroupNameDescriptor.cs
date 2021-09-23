using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class EndpointGroupNameDescriptor : IApiDescriptionProvider
    {
        private readonly IEndpointGroupNameProvider _endpointGroupNameProvider;

        public EndpointGroupNameDescriptor(IEndpointGroupNameProvider endpointGroupNameProvider)
        {
            _endpointGroupNameProvider = endpointGroupNameProvider;
        }

        public int Order => 1000;

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            foreach (var apiDescription in context.Results)
            {
                var endpointMetadata = apiDescription.ActionDescriptor.EndpointMetadata?.OfType<Endpoint>().FirstOrDefault();

                if (endpointMetadata == null)
                {
                    continue;
                }

                apiDescription.GroupName = _endpointGroupNameProvider.GetGroupName(endpointMetadata);
            }
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
        }
    }
}
