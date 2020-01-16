using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class EndpointGroupNameProvider : IEndpointGroupNameProvider
    {
        private readonly IDefaultEndpointGroupNameProvider _defaultEndpointGroupNameProvider;

        public EndpointGroupNameProvider(IDefaultEndpointGroupNameProvider defaultEndpointGroupNameProvider)
        {
            _defaultEndpointGroupNameProvider = defaultEndpointGroupNameProvider;
        }

        public string GetGroupName(Endpoint endpoint)
        {
            if (!string.IsNullOrWhiteSpace(endpoint.GroupName))
            {
                return endpoint.GroupName;
            }

            return _defaultEndpointGroupNameProvider.GetDefaultGroupName(endpoint);
        }
    }
}
