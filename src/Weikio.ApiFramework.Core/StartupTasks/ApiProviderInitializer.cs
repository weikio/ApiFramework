using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.AspNetCore.Common;

namespace Weikio.ApiFramework.Core.StartupTasks
{
    /// <summary>
    /// Startup task which initializes the api definitions. This task takes a <see cref="IApiProvider"/> and then makes sure that the provider is initialized.
    /// This task is automatically run only once when the Api Framework starts.
    /// </summary>
    public class ApiProviderInitializer : IApiProviderInitializer
    {
        private readonly IApiProvider _apiProvider;
        private readonly ILogger<ApiProviderInitializer> _logger;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IEndpointStartupHandler _endpointStartupHandler;
        private readonly ApiFrameworkOptions _options;

        public ApiProviderInitializer(IApiProvider apiProvider,
            ILogger<ApiProviderInitializer> logger, IBackgroundTaskQueue backgroundTaskQueue, IOptions<ApiFrameworkOptions> options, IEndpointStartupHandler endpointStartupHandler)
        {
            _apiProvider = apiProvider;
            _logger = logger;
            _backgroundTaskQueue = backgroundTaskQueue;
            _endpointStartupHandler = endpointStartupHandler;
            _options = options.Value;
        }

        public Task Initialize()
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await Run(cancellationToken);
            });

            return Task.CompletedTask;
        }
        
        private async Task Run(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing the Api provider");
            await _apiProvider.Initialize(cancellationToken).ConfigureAwait(false);

            var allApis = _apiProvider.List();
            _logger.LogDebug($"There's {allApis.Count} apis available:");

            foreach (var apiDefinition in allApis)
            {
                _logger.LogDebug($"{apiDefinition}");
            }
            
            _logger.LogInformation("Api provider initialized");

            _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
            {
                if (_options.AutoInitializeConfiguredEndpoints)
                {
                    _endpointStartupHandler.Start(cancellationToken);
                }
            });
        }
    }
}
