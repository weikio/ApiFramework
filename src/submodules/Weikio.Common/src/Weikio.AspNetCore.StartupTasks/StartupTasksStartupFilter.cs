using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Weikio.AspNetCore.Common;
using Weikio.Common;

namespace Weikio.AspNetCore.StartupTasks
{
    public class StartupTasksStartupFilter : IStartupFilter
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IServiceProvider _serviceProvider;
        private readonly StartupTaskContext _startupTaskContext;
        private readonly IStartupTaskQueue _taskQueue;

        public StartupTasksStartupFilter(IServiceCollection serviceCollection, IServiceProvider serviceProvider,
            StartupTaskContext startupTaskContext, IStartupTaskQueue taskQueue)
        {
            _serviceCollection = serviceCollection;
            _serviceProvider = serviceProvider;
            _startupTaskContext = startupTaskContext;
            _taskQueue = taskQueue;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            var startupTaskTypes = _serviceCollection.GetByInterface(typeof(IStartupTask));

            foreach (var startupTaskType in startupTaskTypes)
            {
                var startupTask = (IStartupTask) _serviceProvider.GetService(startupTaskType);
                var isAutomatic = startupTask.GetPropertyValue<bool>("IsAutomatic", true);

                if (isAutomatic)
                {
                    _startupTaskContext.RegisterTask();
                    _taskQueue.QueueStartupTask(startupTask);
                }
            }

            return app =>
            {
                app.UseHealthChecks("/healtx");

                next(app);
            };
        }
    }
}
