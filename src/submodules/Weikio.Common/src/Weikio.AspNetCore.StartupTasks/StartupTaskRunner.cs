using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Weikio.AspNetCore.StartupTasks
{
    public class StartupTaskRunner : BackgroundService
    {
        private readonly StartupTaskContext _startupTaskContext;
        private readonly IServiceProvider _services;
        private readonly Type _startupTaskType;

        public StartupTaskRunner(StartupTaskContext startupTaskContext, IServiceProvider services, Type startupTaskType)
        {
            _startupTaskContext = startupTaskContext;
            _services = services;
            _startupTaskType = startupTaskType;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var startupTask = (IStartupTask) _services.GetService(_startupTaskType);

            await startupTask.Execute(cancellationToken);

            _startupTaskContext.MarkTaskAsComplete();
        }
    }
}
