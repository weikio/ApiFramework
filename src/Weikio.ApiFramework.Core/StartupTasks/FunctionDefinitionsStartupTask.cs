using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.AspNetCore.StartupTasks;

namespace Weikio.ApiFramework.Core.StartupTasks
{
    /// <summary>
    /// Startup task which initializes the function definitions. This task takes a <see cref="IFunctionProvider"/> and then makes sure that the provider is initialized.
    /// This provider is automatically run only once when the Function Framework starts.
    /// </summary>
    public class FunctionDefinitionsStartupTask : IStartupTask
    {
        private readonly IFunctionProvider _functionProvider;
        private readonly ILogger<FunctionDefinitionsStartupTask> _logger;
        private readonly EndpointStartupTask _endpointStartupTask;
        private readonly StartupTaskContext _startupTaskContext;
        private readonly IStartupTaskQueue _taskQueue;

        public FunctionDefinitionsStartupTask(IFunctionProvider functionProvider,
            ILogger<FunctionDefinitionsStartupTask> logger, EndpointStartupTask endpointStartupTask, StartupTaskContext startupTaskContext,
            IStartupTaskQueue taskQueue)
        {
            _functionProvider = functionProvider;
            _logger = logger;
            _endpointStartupTask = endpointStartupTask;
            _startupTaskContext = startupTaskContext;
            _taskQueue = taskQueue;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing the function provider");

            await _functionProvider.Initialize();

            var allFunctions = await _functionProvider.List();

            _logger.LogDebug($"There's {allFunctions.Count} functions available:");

            foreach (var functionDefinition in allFunctions)
            {
                _logger.LogDebug($"{functionDefinition}");
            }

            _logger.LogInformation("Function provider initialized");

            _startupTaskContext.RegisterTask();
            _taskQueue.QueueStartupTask(_endpointStartupTask);
        }
    }
}
