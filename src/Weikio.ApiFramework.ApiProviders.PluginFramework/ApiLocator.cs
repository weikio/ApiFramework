using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public static class ApiLocator
    {
        public static Func<MetadataReader, TypeDefinition, bool> IsApi = (metadata, type) =>
        {
            var typeName = metadata.GetString(type.Name);

            if (typeName.EndsWith("Api") && !string.Equals(typeName, "Api", StringComparison.InvariantCultureIgnoreCase) &&
                !type.Attributes.HasFlag(TypeAttributes.Abstract))
            {
                return true;
            }

            if (typeName == "ApiFactory" &&
                type.Attributes.HasFlag(TypeAttributes.Abstract | TypeAttributes.Sealed)) // Abstract + Sealed = Static
            {
                var apiFactoryMethods = type.GetMethods()
                    .Select(d => metadata.GetMethodDefinition(d))
                    .Where(m => m.Attributes.HasFlag(MethodAttributes.Public) &&
                                m.Attributes.HasFlag(MethodAttributes.Static) &&
                                metadata.GetString(m.Name) == "Create")
                    .ToArray();

                if (apiFactoryMethods.Any())
                {
                    return true;
                }
            }

            return false;
        };
    }
}
