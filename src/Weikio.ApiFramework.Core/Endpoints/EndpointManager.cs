using System.Collections.Generic;
using System.Linq;
using Weikio.ApiFramework.Abstractions;
using Weikio.AspNetCore.Common;

namespace Weikio.ApiFramework.Core.Endpoints
{
    public class EndpointManager
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly EndpointInitializer _initializer;

        private readonly List<Endpoint> _endpoints;
        public EndpointCollection Endpoints { get; }

        public EndpointManager(IBackgroundTaskQueue backgroundTaskQueue,
            EndpointInitializer initializer)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _initializer = initializer;
            _endpoints = new List<Endpoint>();

            Endpoints = new EndpointCollection(_endpoints);
        }

        public EndpointManagerStatusEnum Status
        {
            get
            {
                if (_endpoints?.Any() != true)
                {
                    return EndpointManagerStatusEnum.Empty;
                }

                if (_endpoints.Any(x => x.Status.Status == EndpointStatusEnum.Initializing))
                {
                    return EndpointManagerStatusEnum.Initializing;
                }

                if (_endpoints.Any(x => x.Status.Status == EndpointStatusEnum.New))
                {
                    return EndpointManagerStatusEnum.Changed;
                }

                if (_endpoints.Any(x => x.Status.Status == EndpointStatusEnum.Failed || x.Status.Status == EndpointStatusEnum.Unhealthy) && _endpoints.Any(x =>
                        x.Status.Status != EndpointStatusEnum.Failed || x.Status.Status != EndpointStatusEnum.Unhealthy))
                {
                    return EndpointManagerStatusEnum.PartiallyRunning;
                }

                if (_endpoints.Any(x => x.Status.Status == EndpointStatusEnum.Failed))
                {
                    return EndpointManagerStatusEnum.Failed;
                }

                return EndpointManagerStatusEnum.Running;
            }
        }

//        public async Task AddEndpoint(IConfigurationSection endpointConfiguration)
//        {
//            var route = endpointConfiguration.Key;
//            var functionName = endpointConfiguration.GetValue<string>("Plugin");
//
//            if (string.IsNullOrEmpty(functionName))
//            {
//                throw new InvalidOperationException($"Plugin is not configured for endpoint '{route}'.");
//            }
//
//            var functionDefinition = await _functionProvider.Get(functionName);
//
//            if (functionDefinition == null)
//            {
//                throw new InvalidOperationException($"Api '{functionName}' is not available.");
//            }
//
//            var endpoint = new Endpoint(route, functionDefinition, null);
//            await endpoint.Initialize();
//
//            AddEndpoint(endpoint);
//        }

        public void AddEndpoint(Endpoint endpoint)
        {
            _endpoints.Add(endpoint);
        }

        /// <summary>
        /// Updates the current runtime status to match the configuration. If new endpoints are added runtime, these are not applied automatically.
        /// </summary>
        public void Update()
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(async x =>
            {
                await _initializer.Initialize(_endpoints);
            });
        }

        public void RemoveEndpoint(Endpoint endpoint)
        {
            _endpoints.Remove(endpoint);
        }

//        public void ReplaceEndpoint(Endpoint endpoint, IConfigurationSection newPluginConfiguration)
//        {
//            RemoveEndpoint(endpoint);
//
//            try
//            {
//                AddEndpoint(newPluginConfiguration);
//            }
//            catch (Exception)
//            {
//                // add the old version back
//                AddEndpoint(endpoint);
//
//                throw;
//            }
//        }
    }
}
