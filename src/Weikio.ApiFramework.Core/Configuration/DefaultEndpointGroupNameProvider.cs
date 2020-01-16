using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Configuration
{
    public class DefaultEndpointGroupNameProvider : IDefaultEndpointGroupNameProvider
    {
        public string GetDefaultGroupName(Endpoint endpoint)
        {
            return "api_framework_endpoint";
        }
    }
}
