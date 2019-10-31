using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddPluginFramework(this IApiFrameworkBuilder builder,
            Action<PluginFrameworkApiProviderOptions> setupAction = null)
        {
            builder.Services.AddSingleton<IApiProvider>(services =>
            {
                var configurationOptions = services.GetService<IOptions<PluginFrameworkApiProviderOptions>>();
                var initializationWrapper = services.GetService<IApiInitializationWrapper>();
                var healthCheckWrapper = services.GetService<IApiHealthCheckWrapper>();
                var logger = services.GetService<ILogger<PluginFrameworkApiProvider>>();
                
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

                var registeredCatalogs = GetRegisteredPluginCatalogs(services);

                var exporter = services.GetService<IPluginExporter>();

                if (options.ApiAssemblies?.Any() == true)
                {
                    var assemblyCatalogs = new List<IPluginCatalog>();

                    foreach (var apiAssembly in options.ApiAssemblies)
                    {
                        var assemblyCatalog = new AssemblyPluginCatalog(apiAssembly);
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
                    var apiProvider = new PluginFrameworkApiProvider(compositeCatalog, exporter, initializationWrapper, healthCheckWrapper, logger);

                    return apiProvider;
                }

                if (options.AutoResolveApis)
                {
                    var binDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    var pluginCatalog = new FolderPluginCatalog(binDirectory, ApiLocator.IsApi);

                    if (!registeredCatalogs.Any())
                    {
                        var apiProvider = new PluginFrameworkApiProvider(pluginCatalog, exporter, initializationWrapper, healthCheckWrapper, logger);

                        return apiProvider;
                    }
                    else
                    {
                        var compositeCatalog = new CompositePluginCatalog(pluginCatalog);

                        foreach (var catalog in registeredCatalogs)
                        {
                            compositeCatalog.AddCatalog(catalog);
                        }

                        var apiProvider = new PluginFrameworkApiProvider(compositeCatalog, exporter, initializationWrapper, healthCheckWrapper, logger);

                        return apiProvider;
                    }
                }

                if (registeredCatalogs.Any())
                {
                    var compositeCatalog = new CompositePluginCatalog(registeredCatalogs);

                    return new PluginFrameworkApiProvider(compositeCatalog, exporter, initializationWrapper, healthCheckWrapper, logger);
                }

                return new PluginFrameworkApiProvider(new EmptyPluginCatalog(), exporter, initializationWrapper, healthCheckWrapper, logger);
            });

            builder.Services.AddTransient<IPluginExporter, PluginExporter>();
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
