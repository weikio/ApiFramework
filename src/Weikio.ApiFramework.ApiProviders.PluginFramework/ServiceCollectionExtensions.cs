using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.ApiFramework.SDK;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.NuGet;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddPluginFramework(this IApiFrameworkBuilder builder,
            Action<PluginFrameworkApiProviderOptions> setupAction = null)
        {
            // Always means "default to host's dlls", then use plugin's references. This is needed because of the DI system.
            // If plugin references older version of package (for example Microsoft.Extensions.Logging.Abstractions) and the host has a newer version
            // and if we prefer the plugin's version, the DI system can't inject the correct version of the ILogger (as it only knows about the host's version).
            // By setting this to always we default to host's assemblies and then revert to plugin's assemblies. This should work well with scenarios where 
            // two plugins use a different version of a package (like Newtonsoft.Json) but the host application doesn't have that reference.
            PluginLoadContextOptions.Defaults.UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Always;
            NugetPluginCatalogOptions.Defaults.LoggerFactory = () => new NugetLogger(builder.Services);

            AssemblyPluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions()
            {
                PluginNameGenerator = (nameOptions, type) =>
                {
                    if (type.GetCustomAttribute(typeof(DisplayNameAttribute), true) is DisplayNameAttribute displayNameAttribute)
                    {
                        return displayNameAttribute.DisplayName;
                    }

                    var assemblyName = type.Assembly.GetName();

                    return assemblyName.Name;
                }
            };

            NugetPluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions()
            {
                PluginNameGenerator = (nameOptions, type) =>
                {
                    if (type.GetCustomAttribute(typeof(DisplayNameAttribute), true) is DisplayNameAttribute displayNameAttribute)
                    {
                        return displayNameAttribute.DisplayName;
                    }

                    if (string.Equals(type.Name, "ApiFactory", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return type.Assembly.GetName().Name;
                    }

                    return type.FullName;
                },
            };
            
            builder.Services.AddSingleton<IApiCatalog>(services =>
            {
                var configurationOptions = services.GetService<IOptions<PluginFrameworkApiProviderOptions>>();
                var initializationWrapper = services.GetService<IApiInitializationWrapper>();
                var healthCheckWrapper = services.GetService<IApiHealthCheckWrapper>();
                var logger = services.GetService<ILogger<PluginFrameworkApiCatalog>>();
                var apiPluginOptions = services.GetService<IOptions<ApiPluginOptions>>().Value;

                PluginFrameworkApiProviderOptions options = null;

                if (configurationOptions != null)
                {
                    options = configurationOptions.Value;
                }
                else
                {
                    options = new PluginFrameworkApiProviderOptions();
                }

                // TODO: Replace with interface/DI based solution
                ApiLocator.IsApi = options.ApiResolver;

                TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();

                foreach (var apiFinderCriterion in options.ApiFinderCriteria)
                {
                    TypeFinderOptions.Defaults.TypeFinderCriterias.Add(apiFinderCriterion);
                }

                var registeredPluginCatalogs = GetRegisteredPluginCatalogs(services);
                var registeredCatalogs = new List<IPluginCatalog>(registeredPluginCatalogs);

                foreach (var apiPluginAssembly in apiPluginOptions.ApiPluginAssemblies)
                {
                    var catalog = new AssemblyPluginCatalog(apiPluginAssembly);
                    registeredCatalogs.Add(catalog);
                }

                if (options.ApiAssemblies?.Any() == true)
                {
                    var assemblyCatalogs = new List<IPluginCatalog>();

                    foreach (var assemblyPath in options.ApiAssemblies)
                    {
                        var assemblyCatalog = new AssemblyPluginCatalog(assemblyPath);
                        assemblyCatalogs.Add(assemblyCatalog);
                    }

                    if (registeredCatalogs.Any())
                    {
                        foreach (var catalog in registeredCatalogs)
                        {
                            assemblyCatalogs.Add(catalog);
                        }
                    }

                    var compositeCatalog = new CompositePluginCatalog(assemblyCatalogs.ToArray());
                    var apiProvider = new PluginFrameworkApiCatalog(compositeCatalog, initializationWrapper, healthCheckWrapper, logger, options);

                    return apiProvider;
                }

                if (options.AutoResolveApis)
                {
                    var binDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    var folderPluginCatalogOptions = new FolderPluginCatalogOptions();

                    var pluginCatalog = new FolderPluginCatalog(binDirectory, folderPluginCatalogOptions);

                    if (!registeredCatalogs.Any())
                    {
                        var apiProvider = new PluginFrameworkApiCatalog(pluginCatalog, initializationWrapper, healthCheckWrapper, logger, options);

                        return apiProvider;
                    }
                    else
                    {
                        var compositeCatalog = new CompositePluginCatalog(pluginCatalog);

                        foreach (var catalog in registeredCatalogs)
                        {
                            compositeCatalog.AddCatalog(catalog);
                        }

                        var apiProvider = new PluginFrameworkApiCatalog(compositeCatalog, initializationWrapper, healthCheckWrapper, logger, options);

                        return apiProvider;
                    }
                }

                if (registeredCatalogs.Any())
                {
                    var compositeCatalog = new CompositePluginCatalog(registeredCatalogs.ToArray());

                    return new PluginFrameworkApiCatalog(compositeCatalog, initializationWrapper, healthCheckWrapper, logger, options);
                }

                return new PluginFrameworkApiCatalog(new EmptyPluginCatalog(), initializationWrapper, healthCheckWrapper, logger, options);
            });

            builder.Services.TryAddSingleton<IApiInitializationWrapper, ApiInitializationWrapper>();
            builder.Services.TryAddSingleton<IApiHealthCheckWrapper, ApiHealthCheckWrapper>();

            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }

            return builder;
        }

        private static IPluginCatalog[] GetRegisteredPluginCatalogs(IServiceProvider services)
        {
            var result = services.GetServices(typeof(IPluginCatalog)).Cast<IPluginCatalog>().ToArray();

            return result;
        }
    }
}
