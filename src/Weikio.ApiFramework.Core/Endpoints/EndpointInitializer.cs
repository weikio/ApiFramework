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
        private readonly ApiFrameworkOptions _options;

        public EndpointInitializer(ILogger<EndpointInitializer> logger, ApiChangeNotifier changeNotifier, IOptions<ApiFrameworkOptions> options, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _logger = logger;
            _changeNotifier = changeNotifier;
            _backgroundTaskQueue = backgroundTaskQueue;
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
        }

        public async Task Initialize(Endpoint endpoint, bool force = false)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
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
        }
    }
}
