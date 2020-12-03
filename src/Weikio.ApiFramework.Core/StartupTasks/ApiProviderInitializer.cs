using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.AspNetCore.Common;
using Weikio.AspNetCore.StartupTasks;

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
        private ApiFrameworkOptions _options;

        public ApiProviderInitializer(IApiProvider apiProvider,
            ILogger<ApiProviderInitializer> logger, IBackgroundTaskQueue backgroundTaskQueue, IOptions<ApiFrameworkOptions> options, IEndpointStartupHandler endpointStartupHandler)
        {
            _apiProvider = apiProvider;
            _logger = logger;
            _backgroundTaskQueue = backgroundTaskQueue;
            _endpointStartupHandler = endpointStartupHandler;
            _options = options.Value;
        }

        public void Initialize()
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(async cancellationToken =>
            {
                await Run(cancellationToken);
            });
        }
        
        private Task Run(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing the Api provider");

            _backgroundTaskQueue.QueueBackgroundWorkItem(async ct =>
            {
                await _apiProvider.Initialize(cancellationToken).ConfigureAwait(false);

                var allApis = await _apiProvider.List();

                _logger.LogDebug($"There's {allApis.Count} apis available:");

                foreach (var apiDefinition in allApis)
                {
                    _logger.LogDebug($"{apiDefinition}");
                }

                _logger.LogInformation("Api provider initialized");

                if (_options.AutoInitializeConfiguredEndpoints)
                {
                    _endpointStartupHandler.Start(cancellationToken);
                }
            });

            return Task.CompletedTask;

        }
    }
}
