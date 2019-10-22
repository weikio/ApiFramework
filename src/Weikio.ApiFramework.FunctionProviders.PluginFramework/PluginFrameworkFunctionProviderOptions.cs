using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Weikio.ApiFramework.FunctionProviders.PluginFramework
{
    public class PluginFrameworkFunctionProviderOptions
    {
        public List<string> FunctionAssemblies { get; set; } = new List<string>();
        public bool AutoResolveFunctions { get; set; }
        public Func<MetadataReader, TypeDefinition, bool> FunctionResolver { get; set; } = FunctionLocator.IsFunction;
    }
}
