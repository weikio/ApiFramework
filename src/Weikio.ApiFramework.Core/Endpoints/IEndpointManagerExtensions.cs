using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Endpoints
{
    public static class IEndpointManagerExtensions
    {
        public static Endpoint Create(this IEndpointManager endpointManager, string route, Api api, object configuration)
        {
            var def = new EndpointDefinition(route, api.ApiDefinition, configuration);
            var result = endpointManager.Create(def);

            return result;
        }

        public static Endpoint CreateAndAdd(this IEndpointManager endpointManager, string route, Api api, object configuration)
        {
            var def = new EndpointDefinition(route, api.ApiDefinition, configuration);
            var result = endpointManager.CreateAndAdd(def);

            return result;
        }
    }
}
