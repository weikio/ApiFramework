using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.ApiProviders.PluginFramework
{
    public class EndpointInitializationOptions
    {
        /// <summary>
        /// Gets or sets the status message for the endpoint. Used by default OnInitializationError to get an updated status for the endpoint.
        /// </summary>
        public Func<Endpoint, Exception, int, Task<string>> GetStatusMessage = (endpoint, exception, count) =>
        {
            return Task.FromResult("Failed to initialize. Trying again shortly. Error: " + exception);
        };

        /// <summary>
        /// Gets or sets the OnBeforeUpdatingInitializationFailedStatus. Can be used to run a task before OnInitializationError happens.
        /// </summary>
        public Func<Endpoint, Exception, int, string, Task> OnBeforeUpdatingInitializationFailedStatus = (endpoint, exception, count, statusMessage) =>
        {
            return Task.CompletedTask;
        };

        /// <summary>
        /// Gets or sets the OnInitializationError. Executed when an exception happens. By defaults creates an error message and updates the endpoint status.
        /// </summary>
        public Func<EndpointInitializationOptions, Endpoint, Exception, TimeSpan, int, ILogger, Task> OnInitializationError =
            async (options, endpoint, exception, interval, errorCount, logger) =>
            {
                var statusMessage = await options.GetStatusMessage(endpoint, exception, errorCount);

                endpoint.Status.UpdateStatus(EndpointStatusEnum.InitializingFailed, statusMessage);

                logger.LogError(exception, $"Failed to initialize endpoint with {endpoint.Route}, trying again in {interval.TotalSeconds} .");
            };
        
        /// <summary>
        /// Gets or sets the OnInitialization. Executed when an initialization happens. By default updates the status of the endpoint if initialization count is more than 1.
        /// </summary>
        public Func<EndpointInitializationOptions, Endpoint, int, ILogger, Task> OnInitialization =
            (options, endpoint, initializationCount, logger) =>
            {
                if (initializationCount > 1)
                {
                    endpoint.Status.UpdateStatus(EndpointStatusEnum.Initializing, $"Initializing, attempt #{initializationCount}");
                }

                return Task.CompletedTask;
            };
        
        /// <summary>
        /// Gets or sets the OnInitialized. Executed after a successful initialization happens. By default logs the event
        /// </summary>
        public Func<EndpointInitializationOptions, Endpoint, ILogger, Task> OnInitialized =
            (options, endpoint, logger) =>
            {
                logger.LogDebug("Initialized {Endpoint} with {Route}", endpoint, endpoint.Route);

                return Task.CompletedTask;
            };

        /// <summary>
        /// Gets or sets the retry count for the default retry policy. Defaults to 3, set to null to retry forever.
        /// </summary>
        public int? RetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry interval for the default retry policy. Defaults to 5 seconds.
        /// </summary>
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets the retry policy. Defaults to WaitAndRetryAsync policy. If retry count is null, defaults to WaitAndRetryForeverAsync.
        /// </summary>
        public Func<EndpointInitializationOptions, Endpoint, ILogger, AsyncRetryPolicy> RetryPolicy = (options, endpoint, logger) =>
        {
            var retryPolicy = Policy
                .Handle<Exception>();

            AsyncRetryPolicy result;

            if (options.RetryCount == null)
            {
                result = retryPolicy.WaitAndRetryForeverAsync(i => options.Interval, async (exception, errorCount, arg3) =>
                {
                    await options.OnInitializationError(options, endpoint, exception, arg3, errorCount, logger);
                });
            }
            else
            {
                result = retryPolicy.WaitAndRetryAsync(options.RetryCount.GetValueOrDefault(), i => options.Interval,
                    async (exception, span, errorCount, arg4) =>
                    {
                        await options.OnInitializationError(options, endpoint, exception, span, errorCount, logger);
                    });
            }

            return result;
        };
    }
}
