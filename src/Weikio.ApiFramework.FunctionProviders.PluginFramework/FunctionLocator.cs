using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Weikio.ApiFramework.FunctionProviders.PluginFramework
{
    public static class FunctionLocator
    {
        public static Func<MetadataReader, TypeDefinition, bool> IsFunction = (metadata, type) =>
        {
            var typeName = metadata.GetString(type.Name);

            if (typeName.EndsWith("Function") && !string.Equals(typeName, "Function", StringComparison.InvariantCultureIgnoreCase) &&
                !type.Attributes.HasFlag(TypeAttributes.Abstract))
            {
                return true;
            }

            if (typeName == "FunctionFactory" &&
                type.Attributes.HasFlag(TypeAttributes.Abstract | TypeAttributes.Sealed)) // Abstract + Sealed = Static
            {
                var functionFactoryMethods = type.GetMethods()
                    .Select(d => metadata.GetMethodDefinition(d))
                    .Where(m => m.Attributes.HasFlag(MethodAttributes.Public) &&
                                m.Attributes.HasFlag(MethodAttributes.Static) &&
                                metadata.GetString(m.Name) == "Create")
                    .ToArray();

                if (functionFactoryMethods.Any())
                {
                    return true;
                }
            }

            return false;
        };
    }
}
