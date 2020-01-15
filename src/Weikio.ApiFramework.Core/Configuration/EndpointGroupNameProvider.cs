using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class EndpointGroupNameProvider : IEndpointGroupNameProvider
    {
        public string GetGroupName(Endpoint endpoint)
        {
            if (!string.IsNullOrWhiteSpace(endpoint.GroupName))
            {
                return endpoint.GroupName;
            }

            return endpoint.Route.Trim('/');
        }
    }
}
