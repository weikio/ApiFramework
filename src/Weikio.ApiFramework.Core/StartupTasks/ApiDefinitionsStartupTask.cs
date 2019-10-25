using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.AspNetCore.StartupTasks;

namespace Weikio.ApiFramework.Core.StartupTasks
{
    /// <summary>
    /// Startup task which initializes the api definitions. This task takes a <see cref="IApiProvider"/> and then makes sure that the provider is initialized.
    /// This provider is automatically run only once when the Api Framework starts.
    /// </summary>
    public class ApiDefinitionsStartupTask : IStartupTask
    {
        private readonly IApiProvider _apiProvider;
        private readonly ILogger<ApiDefinitionsStartupTask> _logger;
        private readonly EndpointStartupTask _endpointStartupTask;
        private readonly StartupTaskContext _startupTaskContext;
        private readonly IStartupTaskQueue _taskQueue;

        public ApiDefinitionsStartupTask(IApiProvider apiProvider,
            ILogger<ApiDefinitionsStartupTask> logger, EndpointStartupTask endpointStartupTask, StartupTaskContext startupTaskContext,
            IStartupTaskQueue taskQueue)
        {
            _apiProvider = apiProvider;
            _logger = logger;
            _endpointStartupTask = endpointStartupTask;
            _startupTaskContext = startupTaskContext;
            _taskQueue = taskQueue;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing the Api provider");

            await _apiProvider.Initialize();

            var allApis = await _apiProvider.List();

            _logger.LogDebug($"There's {allApis.Count} apis available:");

            foreach (var apiDefinition in allApis)
            {
                _logger.LogDebug($"{apiDefinition}");
            }

            _logger.LogInformation("Api provider initialized");

            _startupTaskContext.RegisterTask();
            _taskQueue.QueueStartupTask(_endpointStartupTask);
        }
    }
}
