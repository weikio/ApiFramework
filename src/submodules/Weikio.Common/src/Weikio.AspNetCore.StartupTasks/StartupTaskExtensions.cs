using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.Common;

namespace Weikio.AspNetCore.StartupTasks
{
    public static class StartupTaskExtensions
    {
        private static readonly StartupTaskContext _sharedContext = new StartupTaskContext();

        public static IServiceCollection AddStartupTask<T>(this IServiceCollection services)
            where T : class, IStartupTask
        {
            return services.AddStartupTask(typeof(T));
        }

        public static IServiceCollection AddStartupTask(this IServiceCollection services, Type startupTaskType)
        {
            services.AddTransient(startupTaskType);

            return services;
        }

        public static IServiceCollection AddStartupTasks(this IServiceCollection services, bool autoRegisterStartupTasks = true,
            StartupTasksHealthCheckParameters healthCheckParameters = null)
        {
            services.TryAddSingleton(_sharedContext);
            services.TryAddSingleton(services);

            if (healthCheckParameters == null)
            {
                healthCheckParameters = new StartupTasksHealthCheckParameters();
            }

            services.AddSingleton(healthCheckParameters);

            if (healthCheckParameters.IsEnabled)
            {
                services
                    .AddHealthChecks()
                    .AddCheck<StartupTasksHealthCheck>(healthCheckParameters.HealthCheckName);
            }

            services.AddSingleton<IStartupFilter, StartupTasksStartupFilter>();

            services.AddHostedService<StartupTaskHostedService>();
            services.AddSingleton<IStartupTaskQueue, StartupTaskQueue>();

            if (!autoRegisterStartupTasks)
            {
                return services;
            }

            RegisterStartupTasks(services);

            return services;
        }

        private static void RegisterStartupTasks(IServiceCollection services)
        {
            var startupTaskTypes = TypeLocator.LocateTypesByInterface(typeof(IStartupTask));

            foreach (var startupTaskType in startupTaskTypes)
            {
                services.AddStartupTask(startupTaskType);
            }
        }
    }
}
