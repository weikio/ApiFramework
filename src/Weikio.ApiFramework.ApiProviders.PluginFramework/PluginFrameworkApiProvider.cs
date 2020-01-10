using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    /// <summary>
    /// Api provider which uses Plugin Framework for providing apis
    /// </summary>
    public class PluginFrameworkApiProvider : IApiProvider
    {
        private readonly IPluginCatalog _pluginCatalog;
        private readonly IPluginExporter _exporter;
        private readonly IApiInitializationWrapper _initializationWrapper;
        private readonly IApiHealthCheckWrapper _healthCheckWrapper;
        private readonly ILogger<PluginFrameworkApiProvider> _logger;

        public PluginFrameworkApiProvider(IPluginCatalog pluginCatalog, IPluginExporter exporter, IApiInitializationWrapper initializationWrapper,
            IApiHealthCheckWrapper healthCheckWrapper, ILogger<PluginFrameworkApiProvider> logger)
        {
            _pluginCatalog = pluginCatalog;
            _exporter = exporter;
            _initializationWrapper = initializationWrapper;
            _healthCheckWrapper = healthCheckWrapper;
            _logger = logger;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Initializing PluginFrameworkApiProvider by initializing the {PluginCatalog}", _pluginCatalog);

            // TODO: Make sure that cancellation token can be passed to initialize
            await _pluginCatalog.Initialize();
            IsInitialized = true;

            _logger.LogDebug("PluginFrameworkApiProvider initialized");
        }

        public bool IsInitialized { get; private set; }

        public async Task<List<ApiDefinition>> List()
        {
            if (!IsInitialized)
            {
                return new List<ApiDefinition>();
            }

            var pluginDefinitions = await _pluginCatalog.GetAll();

            var result = new List<ApiDefinition>();

            foreach (var pluginDefinition in pluginDefinitions)
            {
                var apiDefinition = new ApiDefinition(pluginDefinition.Name, pluginDefinition.Version)
                {
                    Description = pluginDefinition.Description, ProductVersion = pluginDefinition.ProductVersion
                };

                result.Add(apiDefinition);
            }

            return result;
        }

        public async Task<Api> Get(ApiDefinition definition)
        {
            _logger.LogDebug("Getting api by {ApiDefinition}", definition);

            var pluginDefinition = await _pluginCatalog.Get(definition.Name, definition.Version);

            if (pluginDefinition == null)
            {
                throw new ApiNotFoundException(definition.Name, definition.Version);
            }

            Dictionary<string, Predicate<Type>> typeTaggers;
            if (pluginDefinition.Source is TypePluginCatalog typePluginCatalog)
            {
                // TODO: This isn't a scalable solution but should work for the first version. In the future it might be that typetaggers should be set per catalog.
                // The problem is that without this if-else handling, TypeCatalog still resolves all the apis in the assembly.

                var bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                         | BindingFlags.Static;
                var field = typePluginCatalog.GetType().GetField("_pluginType", bindFlags);
                var pluginType = (Type) field.GetValue(typePluginCatalog);
                
                typeTaggers = new Dictionary<string, Predicate<Type>>
                {
                    { "Api", type => type.FullName.Equals(pluginType.FullName) && !string.Equals(pluginType.Name, "ApiFactory", StringComparison.InvariantCultureIgnoreCase) },
                    {
                        "Factory", type =>
                        {
                            if (pluginType.Name != "ApiFactory")
                            {
                                return false;
                            }

                            var methods = type.GetMethods().ToList();

                            var factoryMethods = methods
                                .Where(m => m.IsStatic && typeof(Task<IEnumerable<Type>>).IsAssignableFrom(m.ReturnType));

                            return factoryMethods?.Any() == true;
                        }
                    },
                    {
                        "HealthCheck", type => false
                    },
                };
            }
            else
            {
                typeTaggers = new Dictionary<string, Predicate<Type>>
                {
                    { "Api", type => type.Name.EndsWith("Api") },
                    {
                        "Factory", type =>
                        {
                            if (type.Name != "ApiFactory")
                            {
                                return false;
                            }

                            var methods = type.GetMethods().ToList();

                            var factoryMethods = methods
                                .Where(m => m.IsStatic && typeof(Task<IEnumerable<Type>>).IsAssignableFrom(m.ReturnType));

                            return factoryMethods?.Any() == true;
                        }
                    },
                    {
                        "HealthCheck", type =>
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
                };
            }

            var plugin = await _exporter.Get(pluginDefinition, typeTaggers);

            var initializersTypes = plugin.PluginTypes.Where(x => x.Tag == "Factory").Select(x => x.Type).ToList();
            _logger.LogDebug("Found {InitializerTypeCount} initializers for {ApiDefinition}", initializersTypes.Count, definition);
            
            var healthCheckType = plugin.PluginTypes.Where(x => x.Tag == "HealthCheck").Select(x => x.Type).FirstOrDefault();

            if (healthCheckType != null)
            {
                _logger.LogDebug("Found {HealthCheckType} health check for {ApiDefinition}", healthCheckType, definition);
            }

            var initializerMethods = new List<MethodInfo>();

            foreach (var initializerType in initializersTypes)
            {
                initializerMethods.AddRange(initializerType.GetMethods()
                    .Where(m => m.IsStatic && typeof(Task<IEnumerable<Type>>).IsAssignableFrom(m.ReturnType)));
            }

            MethodInfo healthCheckFactoryMethod = null;

            if (healthCheckType != null)
            {
                healthCheckFactoryMethod = healthCheckType.GetMethods().First(m => m.IsStatic && typeof(Task<IHealthCheck>).IsAssignableFrom(m.ReturnType));
            }

            var initializer = _initializationWrapper.Wrap(initializerMethods);
            var healthCheckRunner = _healthCheckWrapper.Wrap(healthCheckFactoryMethod);

            var result = new Api(definition, plugin.PluginTypes.Where(x => x.Tag == "Api").Select(x => x.Type).ToList(),
                initializer, healthCheckRunner);
            
            _logger.LogDebug("Got {Api} from {ApiDefinition}", result, definition);

            return result;
        }

        public async Task<Api> Get(string name, Version version)
        {
            return await Get(new ApiDefinition(name, version));
        }

        public async Task<Api> Get(string name)
        {
            return await Get(name, new Version(1, 0, 0, 0));
        }
    }
}
