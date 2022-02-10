using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.SDK;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public class PluginFrameworkApiProviderOptions
    {
        public List<string> ApiAssemblies { get; set; } = new List<string>();
        public bool AutoResolveApis { get; set; }
        public Func<string, MetadataReader, TypeDefinition, bool> ApiResolver { get; set; } = ApiLocator.IsApi;
        
        /// <summary>
        /// Gets or sets if system feeds should be used as secondary feeds for finding packages when feed url is defined.
        /// </summary>
        public bool IncludeSystemFeedsAsSecondary { get; set; } = true;

        /// <summary>
        /// Gets or sets the criteria which is used when scanning assemblies or directories for APIs
        /// </summary>
        public List<TypeFinderCriteria> ApiFinderCriteria = new List<TypeFinderCriteria>()
        {
            new TypeFinderCriteria()
            {
                Tags = new List<string>() { "Api" },
                Query = (context, type) =>
                    type.Name.EndsWith("Api") && string.Equals("api", type.Name, StringComparison.InvariantCultureIgnoreCase) == false
            },
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
            new TypeFinderCriteria() { Tags = new List<string>() { "Factory" }, Query = (context, type) => type.Name == "ApiFactory" },
        };

        /// <summary>
        /// Gets or sets the func which is used to create the criteria which is used when scanning a single type for APIs
        /// </summary>
        public Func<Type, List<TypeFinderCriteria>> CreateTypeApiFinderCriteria { get; set; } = type =>
        {
            var result = new List<TypeFinderCriteria>();

            var factoryCriteria = new TypeFinderCriteria()
            {
                Query = (context, checkedType) =>
                {
                    if (type.FullName != checkedType.FullName)
                    {
                        return false;
                    }

                    var hasFactoryMethod = HasFactoryMethod(checkedType);

                    return hasFactoryMethod;
                },
                Tags = new List<string>() { "Factory" },
            };

            var apiCriteria = new TypeFinderCriteria()
            {
                Query = (context, checkedType) =>
                {
                    if (type.FullName != checkedType.FullName)
                    {
                        return false;
                    }

                    if (string.Equals("api", type.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return false;
                    }

                    if (type.Name.EndsWith("Api"))
                    {
                        return true;
                    }

                    var hasFactoryMethod = HasFactoryMethod(checkedType);

                    return !hasFactoryMethod;
                },
                Tags = new List<string>() { "Api" },
            };

            result.Add(factoryCriteria);
            result.Add(apiCriteria);

            return result;
        };

        private static bool HasFactoryMethod(Type checkedType)
        {
            var staticFactoryMethod = checkedType
                .GetMethods().FirstOrDefault(m => m.IsStatic && typeof(Task<IEnumerable<Type>>).IsAssignableFrom(m.ReturnType));

            var result = staticFactoryMethod != null && checkedType.IsAbstract && checkedType.IsSealed;

            if (result == false)
            {
                var allMethods = checkedType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                foreach (var methodInfo in allMethods)
                {
                    var methodReturnType = methodInfo.ReturnType;

                    // Not the prettiest solution but is OK for the first release
                    // TODO: This is a duplicate from ApiInitializationWrapper
                    if (methodReturnType == typeof(Type) || methodReturnType == typeof(Task<Type>) ||
                        typeof(IEnumerable<Type>).IsAssignableFrom(methodReturnType) ||
                        typeof(Task<IEnumerable<Type>>).IsAssignableFrom(methodReturnType) ||
                        typeof(Task<List<Type>>).IsAssignableFrom(methodReturnType)||
                        typeof(ApiFactoryResult).IsAssignableFrom(methodReturnType))
                    {
                        result = true;

                        break;
                    }
                }
            }

            return result;
        }
    }
}
