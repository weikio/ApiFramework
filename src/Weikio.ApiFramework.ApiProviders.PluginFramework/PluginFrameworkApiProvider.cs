using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.SDK;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    /// <summary>
    /// Api provider which uses Plugin Framework for providing apis
    /// </summary>
    public class PluginFrameworkApiProvider : IApiProvider
    {
        private readonly IPluginCatalog _pluginCatalog;
        private readonly IApiInitializationWrapper _factoryWrapper;
        private readonly IApiHealthCheckWrapper _healthCheckWrapper;
        private readonly ILogger<PluginFrameworkApiProvider> _logger;

        public PluginFrameworkApiProvider(IPluginCatalog pluginCatalog, IApiInitializationWrapper factoryWrapper,
            IApiHealthCheckWrapper healthCheckWrapper, ILogger<PluginFrameworkApiProvider> logger)
        {
            _pluginCatalog = pluginCatalog;
            _factoryWrapper = factoryWrapper;
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

        public List<ApiDefinition> List()
        {
            if (!IsInitialized)
            {
                return new List<ApiDefinition>();
            }

            // In Plugin Framework each type is a plugin. In Api Framework each Api can contain multiple types. So we combine the plugins at this point.
            var pluginDefinitions = _pluginCatalog.GetPlugins().GroupBy(x => new { x.Name, x.Version });

            var result = new List<ApiDefinition>();

            foreach (var pluginDefinition in pluginDefinitions)
            {
                var apiDefinition = new ApiDefinition(pluginDefinition.Key.Name, pluginDefinition.Key.Version)
                {
                    Description = pluginDefinition.First().Description, ProductVersion = pluginDefinition.First().ProductVersion
                };

                result.Add(apiDefinition);
            }

            return result;
        }

        public Api Get(ApiDefinition definition)
        {
            try
            {
                _logger.LogDebug("Getting api by {ApiDefinition}", definition);

                var result = GetApiByDefinition(definition);

                if (result == null)
                {
                    throw new ApiNotFoundException(definition.Name, definition.Version);
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get api using definition {ApiDefinition}", definition);

                _logger.LogDebug("Available APIs:");

                var allApis = List();

                foreach (var api in allApis)
                {
                    _logger.LogDebug(api.ToString());
                }

                throw;
            }
        }

        public Api Get(string name, Version version)
        {
            return Get(new ApiDefinition(name, version));
        }

        public Api Get(string name)
        {
            return Get(name, new Version(1, 0, 0, 0));
        }

        private Api GetApiByDefinition(ApiDefinition apiDefinition)
        {
            var allPlugins = _pluginCatalog.GetPlugins();

            try
            {
                // In Plugin Framework each type is a plugin. In Api Framework each Api can contain multiple types. So we combine the plugins at this point.
                var pluginsForApi = allPlugins.Where(x =>
                    string.Equals(x.Assembly.GetName().Name, apiDefinition.Name, StringComparison.InvariantCultureIgnoreCase) && apiDefinition.Version == x.Version).ToList();

                if (pluginsForApi.Any() != true)
                {
                    throw new PluginForApiNotFoundException(apiDefinition.Name, apiDefinition.Version);
                }

                var factoryTypes = pluginsForApi.Where(x => x.Tags?.Contains("Factory") == true).ToList();
                _logger.LogDebug("Found {FactoryTypeCount} factories for {ApiDefinition}", factoryTypes.Count, apiDefinition);

                // Only one health check for api is supported
                var healthCheckType = pluginsForApi.FirstOrDefault(x => x.Tags?.Contains("HealthCheck") == true);

                if (healthCheckType != null)
                {
                    _logger.LogDebug("Found {HealthCheckType} health check for {ApiDefinition}", healthCheckType, apiDefinition);
                }

                MethodInfo healthCheckFactoryMethod = null;

                if (healthCheckType != null)
                {
                    healthCheckFactoryMethod = healthCheckType.Type.GetMethods()
                        .First(m => m.IsStatic && typeof(Task<IHealthCheck>).IsAssignableFrom(m.ReturnType));
                }

                var factory = _factoryWrapper.Wrap(factoryTypes.Select(x => x.Type).ToList());
                var healthCheckRunner = _healthCheckWrapper.Wrap(healthCheckFactoryMethod);

                var apiTypes = pluginsForApi.Where(x => x.Tags?.Contains("Api") == true).ToList();

                var result = new Api(apiDefinition, apiTypes.Select(x => x.Type).ToList(),
                    factory, healthCheckRunner);

                return result;
            }
            catch (ApiNotFoundException e)
            {
                _logger.LogError(e, "No plugins were found for Api with ApiDefintion {ApiDefinition}.", apiDefinition);
                _logger.LogDebug("Available plugins:");

                foreach (var plugin in allPlugins)
                {
                    _logger.LogDebug(plugin.ToString());
                }

                return null;
            }
        }
    }
}
