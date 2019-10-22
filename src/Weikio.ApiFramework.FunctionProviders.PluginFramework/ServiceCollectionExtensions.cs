using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.PluginFramework;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.ApiFramework.FunctionProviders.PluginFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IFunctionFrameworkBuilder AddPluginFramework(this IFunctionFrameworkBuilder builder,
            Action<PluginFrameworkFunctionProviderOptions> setupAction = null)
        {
            builder.Services.AddSingleton<IFunctionProvider>(services =>
            {
                var configurationOptions = services.GetService<IOptions<PluginFrameworkFunctionProviderOptions>>();
                var initializationWrapper = services.GetService<IFunctionInitializationWrapper>();
                var healthCheckWrapper = services.GetService<IFunctionHealthCheckWrapper>();

                PluginFrameworkFunctionProviderOptions options = null;

                if (configurationOptions != null)
                {
                    options = configurationOptions.Value;
                }
                else
                {
                    options = new PluginFrameworkFunctionProviderOptions();
                }

                // TODO: Replace with interface/DI based solution
                FunctionLocator.IsFunction = options.FunctionResolver;

                var registeredCatalogs = GetRegisteredPluginCatalogs(services);

                var exporter = services.GetService<IPluginExporter>();

                if (options.FunctionAssemblies?.Any() == true)
                {
                    var assemblyCatalogs = new List<IPluginCatalog>();

                    foreach (var functionAssembly in options.FunctionAssemblies)
                    {
                        var assemblyCatalog = new AssemblyPluginCatalog(functionAssembly);
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
                    var functionProvider = new PluginFrameworkFunctionProvider(compositeCatalog, exporter, initializationWrapper, healthCheckWrapper);

                    return functionProvider;
                }

                if (options.AutoResolveFunctions)
                {
                    var binDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    var pluginCatalog = new FolderPluginCatalog(binDirectory, FunctionLocator.IsFunction);

                    if (!registeredCatalogs.Any())
                    {
                        var functionProvider = new PluginFrameworkFunctionProvider(pluginCatalog, exporter, initializationWrapper, healthCheckWrapper);

                        return functionProvider;
                    }

                    else
                    {
                        var compositeCatalog = new CompositePluginCatalog(pluginCatalog);

                        foreach (var catalog in registeredCatalogs)
                        {
                            compositeCatalog.AddCatalog(catalog);
                        }

                        var functionProvider = new PluginFrameworkFunctionProvider(compositeCatalog, exporter, initializationWrapper, healthCheckWrapper);

                        return functionProvider;
                    }
                }

                if (registeredCatalogs.Any())
                {
                    var compositeCatalog = new CompositePluginCatalog(registeredCatalogs);

                    return new PluginFrameworkFunctionProvider(compositeCatalog, exporter, initializationWrapper, healthCheckWrapper);
                }

                return new PluginFrameworkFunctionProvider(new EmptyPluginCatalog(), exporter, initializationWrapper, healthCheckWrapper);
            });

            builder.Services.AddTransient<IPluginExporter, PluginExporter>();
            builder.Services.TryAddSingleton<IFunctionInitializationWrapper, FunctionInitializationWrapper>();
            builder.Services.TryAddSingleton<IFunctionHealthCheckWrapper, FunctionHealthCheckWrapper>();

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
