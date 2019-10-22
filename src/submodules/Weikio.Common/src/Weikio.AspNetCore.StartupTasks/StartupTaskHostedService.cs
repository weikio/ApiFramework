using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace Weikio.AspNetCore.StartupTasks
{
    public class StartupTaskHostedService : BackgroundService
    {
        private readonly StartupTaskContext _startupTaskContext;
        private readonly ILogger _logger;

        public StartupTaskHostedService(IStartupTaskQueue taskQueue, ILoggerFactory loggerFactory,
            StartupTaskContext startupTaskContext)
        {
            _startupTaskContext = startupTaskContext;
            TaskQueue = taskQueue;
            _logger = loggerFactory.CreateLogger<StartupTaskHostedService>();
        }

        public IStartupTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                $"Startup task Hosted Service is starting. {TaskQueue.Count} startup tasks registered.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var startupTask = TaskQueue.DequeueAsync();

                if (startupTask == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                    continue;
                }

                try
                {
                    var retryPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2), (exception, span, arg3, arg4) =>
                        {
                            _logger.LogInformation(
                                $"Failed to run startup task {nameof(startupTask)}, trying again in {span.TotalSeconds} .");
                        });

                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        await startupTask.Execute(cancellationToken);
                        _startupTaskContext.MarkTaskAsComplete();
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred executing startup task {nameof(startupTask)}.");
                }
            }

            _logger.LogInformation("Startup task Hosted Service is stopping.");
        }
    }
}
