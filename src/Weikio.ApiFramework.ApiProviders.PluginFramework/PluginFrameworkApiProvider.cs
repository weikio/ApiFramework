using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;
using Weikio.PluginFramework.Abstractions;

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

        public PluginFrameworkApiProvider(IPluginCatalog pluginCatalog, IPluginExporter exporter, IApiInitializationWrapper initializationWrapper,
            IApiHealthCheckWrapper healthCheckWrapper)
        {
            _pluginCatalog = pluginCatalog;
            _exporter = exporter;
            _initializationWrapper = initializationWrapper;
            _healthCheckWrapper = healthCheckWrapper;
        }

        public async Task Initialize()
        {
            await _pluginCatalog.Initialize();
            IsInitialized = true;
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
                var apiDefinition = new ApiDefinition(pluginDefinition.Name, pluginDefinition.Version);
                result.Add(apiDefinition);
            }

            return result;
        }

        public async Task<Api> Get(ApiDefinition definition)
        {
            var pluginDefinition = await _pluginCatalog.Get(definition.Name, definition.Version);

            if (pluginDefinition == null)
            {
                throw new ApiNotFoundException(definition.Name, definition.Version);
            }

            var typeTaggers = new Dictionary<string, Predicate<Type>>
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

            var plugin = await _exporter.Get(pluginDefinition, typeTaggers);

            var initializersTypes = plugin.PluginTypes.Where(x => x.Tag == "Factory").Select(x => x.Type).ToList();
            var healthCheckType = plugin.PluginTypes.Where(x => x.Tag == "HealthCheck").Select(x => x.Type).FirstOrDefault();

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

            var api = new Api(definition, plugin.PluginTypes.Where(x => x.Tag == "Api").Select(x => x.Type).ToList(),
                initializer, healthCheckRunner);

            return api;
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
