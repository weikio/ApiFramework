using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Weikio.Common
{
    public class TypeLocator
    {
        public static List<Type> LocateTypesByInterface(Type interfaceType)
        {
            var runningDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var dllFiles = Directory.GetFiles(runningDirectory, "*.dll");

            var result = new List<Type>();

            foreach (var dllFile in dllFiles)
            {
                var typesInAssembly = GetImplementingTypes(dllFile, interfaceType);

                if (typesInAssembly?.Any() != true)
                {
                    continue;
                }

                try
                {
                    var assembly = Assembly.LoadFrom(dllFile);

                    foreach (var typeDefinition in typesInAssembly)
                    {
                        var implementationType = assembly.GetType(typeDefinition);
                        result.Add(implementationType);
                    }
                }
                catch (Exception e)
                {
                    // TODO: Better error handling
                    Console.WriteLine(e);

                    throw;
                }
            }

            return result;
        }

        private static List<string> GetImplementingTypes(string assemblyPath, Type interfaceType)
        {
            var result = new List<string>();

            using (Stream stream = File.OpenRead(assemblyPath))
            using (var reader = new PEReader(stream))
            {
                if (!reader.HasMetadata)
                {
                    return result;
                }

                var metadata = reader.GetMetadataReader();

                var publicTypes = metadata.TypeDefinitions
                    .Select(metadata.GetTypeDefinition)
                    .Where(t => t.Attributes.HasFlag(TypeAttributes.Public) &&
                                !t.Attributes.HasFlag(TypeAttributes.Abstract))
                    .ToArray();

                foreach (var type in publicTypes)
                {
                    var na = metadata.GetString(type.Name);
                    var interfaceImplementations = type.GetInterfaceImplementations();

                    foreach (var interfaceImplementation in interfaceImplementations)
                    {
                        var implementation = metadata.GetInterfaceImplementation(interfaceImplementation);
                        var entityHandle = implementation.Interface;

                        var kind = entityHandle.Kind;

                        if (kind != HandleKind.TypeDefinition && kind != HandleKind.TypeReference)
                        {
                            continue;
                        }

                        var interfaceName = "";
                        var interfaceNamespace = "";

                        if (kind == HandleKind.TypeDefinition)
                        {
                            var typeDefinitionHandle = (TypeDefinitionHandle) entityHandle;
                            var typeDefinition = metadata.GetTypeDefinition(typeDefinitionHandle);

                            interfaceName = metadata.GetString(typeDefinition.Name);
                            interfaceNamespace = metadata.GetString(typeDefinition.Namespace);
                        }
                        else
                        {
                            var typeReferenceHandle = (TypeReferenceHandle) entityHandle;
                            var typeDefinition = metadata.GetTypeReference(typeReferenceHandle);

                            interfaceName = metadata.GetString(typeDefinition.Name);
                            interfaceNamespace = metadata.GetString(typeDefinition.Namespace);
                        }

                        var interfaceFullName = string.Join('.', interfaceNamespace, interfaceName);

                        if (!string.Equals(interfaceFullName, interfaceType.FullName))
                        {
                            continue;
                        }

                        var typeName = metadata.GetString(type.Name);
                        var typeNamespace = metadata.GetString(type.Namespace);

                        var typeFullName = string.Join('.', typeNamespace, typeName);

                        result.Add(typeFullName);
                    }
                }
            }

            return result;
        }
    }
}
