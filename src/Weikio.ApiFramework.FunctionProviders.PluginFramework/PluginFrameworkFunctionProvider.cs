using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Weikio.ApiFramework.Abstractions;
using Weikio.PluginFramework;

namespace Weikio.ApiFramework.FunctionProviders.PluginFramework
{
    /// <summary>
    /// Function provider which uses Plugin Framework for providing functions
    /// </summary>
    public class PluginFrameworkFunctionProvider : IFunctionProvider
    {
        private readonly IPluginCatalog _pluginCatalog;
        private readonly IPluginExporter _exporter;
        private readonly IFunctionInitializationWrapper _initializationWrapper;
        private readonly IFunctionHealthCheckWrapper _healthCheckWrapper;

        public PluginFrameworkFunctionProvider(IPluginCatalog pluginCatalog, IPluginExporter exporter, IFunctionInitializationWrapper initializationWrapper,
            IFunctionHealthCheckWrapper healthCheckWrapper)
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

        public async Task<List<FunctionDefinition>> List()
        {
            if (!IsInitialized)
            {
                return new List<FunctionDefinition>();
            }

            var pluginDefinitions = await _pluginCatalog.GetAll();

            var result = new List<FunctionDefinition>();

            foreach (var pluginDefinition in pluginDefinitions)
            {
                var functionDefinition = new FunctionDefinition(pluginDefinition.Name, pluginDefinition.Version);
                result.Add(functionDefinition);
            }

            return result;
        }

        public async Task<Function> Get(FunctionDefinition definition)
        {
            var pluginDefinition = await _pluginCatalog.Get(definition.Name, definition.Version);

            if (pluginDefinition == null)
            {
                throw new FunctionNotFoundException(definition.Name, definition.Version);
            }

            var typeTaggers = new Dictionary<string, Predicate<Type>>
            {
                { "Function", type => type.Name.EndsWith("Function") },
                {
                    "Factory", type =>
                    {
                        if (type.Name != "FunctionFactory")
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

            var function = new Function(definition, plugin.PluginTypes.Where(x => x.Tag == "Function").Select(x => x.Type).ToList(),
                initializer, healthCheckRunner);

            return function;
        }

        public async Task<Function> Get(string name, Version version)
        {
            return await Get(new FunctionDefinition(name, version));
        }

        public async Task<Function> Get(string name)
        {
            return await Get(name, new Version(1, 0, 0, 0));
        }
    }
}
