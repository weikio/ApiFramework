using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    /// <summary>
    /// Api provider which uses Plugin Framework for providing apis
    /// </summary>
    public class PluginFrameworkApiCatalog : IApiCatalog
    {
        private readonly IPluginCatalog _pluginCatalog;
        private readonly IApiInitializationWrapper _factoryWrapper;
        private readonly IApiHealthCheckWrapper _healthCheckWrapper;
        private readonly ILogger<PluginFrameworkApiCatalog> _logger;
        private readonly PluginFrameworkApiProviderOptions _options;

        public PluginFrameworkApiCatalog(IPluginCatalog pluginCatalog, IApiInitializationWrapper factoryWrapper,
            IApiHealthCheckWrapper healthCheckWrapper, ILogger<PluginFrameworkApiCatalog> logger, PluginFrameworkApiProviderOptions options)
        {
            _pluginCatalog = pluginCatalog;
            _factoryWrapper = factoryWrapper;
            _healthCheckWrapper = healthCheckWrapper;
            _logger = logger;
            _options = options;
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
            var pluginsInCatalog = _pluginCatalog.GetPlugins().Where(x => x.Tags?.Contains("HealthCheck") == false).ToList();
            var pluginDefinitions = pluginsInCatalog.GroupBy(x => new { x.Name, x.Version }).ToList();

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
                    return null;
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
                    string.Equals(x.Name, apiDefinition.Name, StringComparison.InvariantCultureIgnoreCase) && apiDefinition.Version == x.Version).ToList();

                if (pluginsForApi.Any() != true)
                {
                    return null;
                }

                // It could be possible that there is multiple assemblies in a single API but that is unlikely
                var assembly = pluginsForApi.First().Assembly;
                
                var factoryPlugins = pluginsForApi.Where(x => x.Tags?.Contains("Factory") == true).ToList();
                _logger.LogDebug("Found {FactoryTypeCount} factories for {ApiDefinition}", factoryPlugins.Count, apiDefinition);

                // Only one health check for api is supported. We try to locate a HealthCheck which resides in the same Assembly as the plugin
                var allHealthChecks = allPlugins.Where(x => x.Tags?.Contains("HealthCheck") == true);
                var healthCheckType = allHealthChecks.FirstOrDefault(x => x.Assembly == assembly);
                
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

                var factory = _factoryWrapper.Wrap(factoryPlugins);
                var healthCheckRunner = _healthCheckWrapper.Wrap(healthCheckFactoryMethod);

                var apiTypes = pluginsForApi.Where(x => x.Tags?.Contains("Api") == true).ToList();
                _logger.LogDebug("Found {ApiTypeCount} api types for {ApiDefinition}", apiTypes.Count, apiDefinition);

                if (factoryPlugins?.Any() != true && apiTypes?.Any() != true)
                {
                    _logger.LogWarning("No api types or factory types found for api definition {ApiDefinition}. It's not possible to use this as an API.", apiDefinition);
                }

                var result = new Api(apiDefinition, apiTypes.Select(x => x.Type).ToList(), assembly, 
                    factory, healthCheckRunner);

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get API by definition {ApiDefinition}.", apiDefinition);

                throw;
            }
        }
    }
}
