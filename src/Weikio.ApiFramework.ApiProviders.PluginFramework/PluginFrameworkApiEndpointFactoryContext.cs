using Weikio.ApiFramework.SDK;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public class PluginFrameworkApiEndpointFactoryContext : ApiEndpointFactoryContext
    {
        public PluginAssemblyLoadContext PluginAssemblyLoadContext
        {
            get
            {
                return AssemblyLoadContext as PluginAssemblyLoadContext;
            }
        } 
        
        public Plugin Plugin { get; set; }
    }
}
