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
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddPluginFramework(this IApiFrameworkBuilder builder,
            Action<PluginFrameworkApiProviderOptions> setupAction = null)
        {
            PluginLoadContextOptions.Defaults.UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Always;

            builder.Services.Configure<PluginNameAndVersionOptions>(typeof(AssemblyPluginCatalog).FullName, options =>
            {
                options.NameOptions.PluginNameGenerator = (nameOptions, type) =>
                {
                    var displayNameAttribute = type.GetCustomAttribute(typeof(DisplayNameAttribute), true) as DisplayNameAttribute;

                    if (displayNameAttribute != null)
                    {
                        return displayNameAttribute.DisplayName;
                    }

                    var assemblyName = type.Assembly.GetName();

                    return assemblyName.Name;
                };
            });

            builder.Services.Replace(ServiceDescriptor.Singleton<IApiProvider>(services =>
            {
                var configurationOptions = services.GetService<IOptions<PluginFrameworkApiProviderOptions>>();
                var initializationWrapper = services.GetService<IApiInitializationWrapper>();
                var healthCheckWrapper = services.GetService<IApiHealthCheckWrapper>();
                var logger = services.GetService<ILogger<PluginFrameworkApiProvider>>();
                var apiPlugins = services.GetServices<ApiPlugin>();
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
                    var apiProvider = new PluginFrameworkApiProvider(compositeCatalog, initializationWrapper, healthCheckWrapper, logger);

                    return apiProvider;
                }

                if (options.AutoResolveApis)
                {
                    var binDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    var folderPluginCatalogOptions = new FolderPluginCatalogOptions();

                    var pluginCatalog = new FolderPluginCatalog(binDirectory, folderPluginCatalogOptions);

                    if (!registeredCatalogs.Any())
                    {
                        var apiProvider = new PluginFrameworkApiProvider(pluginCatalog, initializationWrapper, healthCheckWrapper, logger);

                        return apiProvider;
                    }
                    else
                    {
                        var compositeCatalog = new CompositePluginCatalog(pluginCatalog);

                        foreach (var catalog in registeredCatalogs)
                        {
                            compositeCatalog.AddCatalog(catalog);
                        }

                        var apiProvider = new PluginFrameworkApiProvider(compositeCatalog, initializationWrapper, healthCheckWrapper, logger);

                        return apiProvider;
                    }
                }

                if (registeredCatalogs.Any())
                {
                    var compositeCatalog = new CompositePluginCatalog(registeredCatalogs.ToArray());

                    return new PluginFrameworkApiProvider(compositeCatalog, initializationWrapper, healthCheckWrapper, logger);
                }

                return new PluginFrameworkApiProvider(new EmptyPluginCatalog(), initializationWrapper, healthCheckWrapper, logger);
            }));

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
