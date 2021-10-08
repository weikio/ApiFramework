using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.SDK
{
    public class ApiEndpointFactoryContext
    {
        public Endpoint Endpoint { get; set; }
        public object AssemblyLoadContext { get; set; }
    }
}
