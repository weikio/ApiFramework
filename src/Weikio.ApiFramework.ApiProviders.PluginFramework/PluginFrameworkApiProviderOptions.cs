using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public class PluginFrameworkApiProviderOptions
    {
        public List<string> ApiAssemblies { get; set; } = new List<string>();
        public bool AutoResolveApis { get; set; }
        public Func<string, MetadataReader, TypeDefinition, bool> ApiResolver { get; set; } = ApiLocator.IsApi;
    }
}
