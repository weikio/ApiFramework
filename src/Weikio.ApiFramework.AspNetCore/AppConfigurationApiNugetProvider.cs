using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.ApiFramework.Core.HealthChecks;

namespace Weikio.ApiFramework.AspNetCore
{
    public class AppConfigurationApiNugetStartupService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppConfigurationApiNugetStartupService> _logger;
        private readonly IApiProvider _apiProvider;

        public AppConfigurationApiNugetStartupService(IConfiguration configuration, IServiceProvider serviceProvider, 
            ILogger<AppConfigurationApiNugetStartupService> logger, IApiProvider apiProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _apiProvider = apiProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Reading APIs from configuration");
            var apiFrameworkConfigurationSection = _configuration.GetSection("ApiFramework");
            var hasConfig = apiFrameworkConfigurationSection?.GetChildren()?.Any() == true;

            if (hasConfig == false)
            {
                _logger.LogDebug("No API Framework configuration section found");
                return Task.CompletedTask;
            }
            
            var apisection = apiFrameworkConfigurationSection?.GetSection("Apis");

            if (apisection == null)
            {
                _logger.LogDebug("No Apis section found in API Framework configuration");
                return Task.CompletedTask;
            }

            var apiCount = 0;
            foreach (var api in apisection.GetChildren())
            {
                var packageName = api.GetValue<string>("Name");
                var version = api.GetValue<string>("Version");
                var feedUrl = api.GetValue<string>("FeedUrl");
                
                var catalog = NugetPackageFactory.CreateApiCatalog(packageName, version, _serviceProvider, feedUrl);
                
                _apiProvider.Add(catalog);

                apiCount += 1;
                
                _logger.LogDebug("Adding API {ApiName}, with version {ApiVersion}", packageName, version);
            }

            if (apiCount < 1)
            {
                _logger.LogDebug("No apis defined in the Apis section of the API Framework configuration");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
