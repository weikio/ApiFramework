using System.Collections.Generic;
using System.Reflection;

namespace Weikio.ApiFramework.SDK
{
    public class ApiPluginOptions
    {
        public List<Assembly> ApiPluginAssemblies { get; set; } = new List<Assembly>();
    }
}