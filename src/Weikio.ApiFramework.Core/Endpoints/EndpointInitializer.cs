using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.AspNetCore.Common;

namespace Weikio.ApiFramework.Core.Endpoints
{
    public class EndpointInitializer : IEndpointInitializer
    {
        private readonly ILogger<EndpointInitializer> _logger;
        private readonly ApiChangeNotifier _changeNotifier;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IApiConfigurationTypeProvider _apiConfigurationTypeProvider;
        private readonly IEnumerable<IEndpointStatusObserverFactory> _statusObserversFactories;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApiFrameworkOptions _options;
        private static string _initializationLock = "lock";

        public EndpointInitializer(ILogger<EndpointInitializer> logger, ApiChangeNotifier changeNotifier, IOptions<ApiFrameworkOptions> options,
            IBackgroundTaskQueue backgroundTaskQueue, IApiConfigurationTypeProvider apiConfigurationTypeProvider, 
            IEnumerable<IEndpointStatusObserverFactory> statusObserversFactories, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _changeNotifier = changeNotifier;
            _backgroundTaskQueue = backgroundTaskQueue;
            _apiConfigurationTypeProvider = apiConfigurationTypeProvider;
            _statusObserversFactories = statusObserversFactories;
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public void Initialize(List<Endpoint> endpoints, bool force = false)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            if (endpoints?.Any() != true)
            {
                return;
            }

            lock (_initializationLock)
            {
                _logger.LogDebug("Initializing {EndpointCount} {Endpoints}", endpoints.Count, endpoints);

                _backgroundTaskQueue.QueueBackgroundWorkItem(async cancellationToken =>
                {
                    await endpoints.ForEachAsync(async endpoint =>
                    {
                        await Initialize(endpoint, force);
                    });

                    if (_options.ChangeNotificationType == ChangeNotificationTypeEnum.Batch)
                    {
                        _changeNotifier.Notify();
                    }
                });

                _logger.LogDebug("Endpoints initialized");
            }
        }

        public async Task Initialize(Endpoint endpoint, bool force = false)
        {
            _logger.LogDebug("Initializing {Endpoint}. Force: {Force}", endpoint, force);

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }
            
            // Attach status observers
            if (endpoint.Status.HasStatusObserver == false)
            {
                var observerCount = 0;
                foreach (var factory in _statusObserversFactories)
                {
                    var observer = factory.Create(_serviceProvider, endpoint);
                    endpoint.Status.AddObserver(observer);

                    observerCount += 1;
                }

                if (observerCount > 0)
                {
                    _logger.LogDebug("Added {Count} status observers to Endpoint {Endpoint}", observerCount, endpoint);
                }
                else
                {
                    _logger.LogTrace("No status observers available for Endpoint {Endpoint}", endpoint);
                }
            }

            var endpointStatus = endpoint.Status;

            if (force == false && (endpointStatus.Status == EndpointStatusEnum.Ready || endpointStatus.Status == EndpointStatusEnum.Failed))
            {
                return;
            }

            await endpoint.Initialize();
            
            if (_options.ChangeNotificationType == ChangeNotificationTypeEnum.Single)
            {
                _changeNotifier.Notify();
            }

            // Also update the known configuration types
            if (endpoint.Configuration == null)
            {
                return;
            }
            
            var currentConfigurationType = _apiConfigurationTypeProvider.GetByApi(endpoint.Api.ApiDefinition);

            if (currentConfigurationType != null)
            {
                return;
            }

            var configurationType = new ApiConfiguration(endpoint.Api.ApiDefinition, endpoint.Configuration.GetType());
            _apiConfigurationTypeProvider.Add(configurationType);
        }
    }
}
