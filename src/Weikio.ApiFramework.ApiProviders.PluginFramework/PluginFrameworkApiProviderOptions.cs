using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public class PluginFrameworkApiProviderOptions
    {
        public List<string> ApiAssemblies { get; set; } = new List<string>();
        public bool AutoResolveApis { get; set; }
        public Func<string, MetadataReader, TypeDefinition, bool> ApiResolver { get; set; } = ApiLocator.IsApi;

        public List<TypeFinderCriteria> ApiFinderCriteria = new List<TypeFinderCriteria>()
        {
            new TypeFinderCriteria() { Tags = new List<string>() { "Api" }, Query = (context, type) => type.Name.EndsWith("Api") },
            new TypeFinderCriteria()
            {
                Tags = new List<string>() { "HealthCheck" },
                Query = (context, type) =>
                {
                    if (type.Name != "HealthCheckFactory")
                    {
                        return false;
                    }

                    var methods = type.GetMethods().ToList();

                    var factoryMethods = methods
                        .Where(m => m.IsStatic && typeof(Task<IHealthCheck>).IsAssignableFrom(m.ReturnType));

                    return factoryMethods?.Any() == true;
                }
            },
            new TypeFinderCriteria()
            {
                Tags = new List<string>() { "Factory" },
                Query = (context, type) => type.Name == "ApiFactory"
            },
        };
    }
}
