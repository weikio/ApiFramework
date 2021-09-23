using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public static class ApiLocator
    {
        public static Func<string, MetadataReader, TypeDefinition, bool> IsApi = (assembly, metadata, type) =>
        {
            var typeName = metadata.GetString(type.Name);

            if (typeName.EndsWith("Api") && !string.Equals(typeName, "Api", StringComparison.InvariantCultureIgnoreCase) &&
                !type.Attributes.HasFlag(TypeAttributes.Abstract))
            {
                return true;
            }

            if (typeName != "ApiFactory" || !type.Attributes.HasFlag(TypeAttributes.Abstract | TypeAttributes.Sealed))
            {
                return false;
            }

            var apiFactoryMethods = type.GetMethods()
                .Select(metadata.GetMethodDefinition)
                .Where(m => m.Attributes.HasFlag(MethodAttributes.Public) &&
                            m.Attributes.HasFlag(MethodAttributes.Static) &&
                            metadata.GetString(m.Name) == "Create")
                .ToArray();

            return apiFactoryMethods.Any();
        };
    }
}
