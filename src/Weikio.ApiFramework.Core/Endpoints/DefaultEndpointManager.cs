﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.Infrastructure;

namespace Weikio.ApiFramework.Core.Endpoints
{
    public class DefaultEndpointManager : List<Endpoint>, IEndpointManager
    {
        private readonly IEndpointInitializer _initializer;
        private readonly IApiProvider _apiProvider;
        private readonly ILogger<DefaultEndpointManager> _logger;
        private readonly ApiChangeNotifier _changeNotifier;

        public List<Endpoint> Endpoints => this;

        public DefaultEndpointManager(IEndpointInitializer initializer, IApiProvider apiProvider, ILogger<DefaultEndpointManager> logger,
            ApiChangeNotifier changeNotifier)
        {
            _initializer = initializer;
            _apiProvider = apiProvider;
            _logger = logger;
            _changeNotifier = changeNotifier;
        }

        public EndpointManagerStatusEnum Status
        {
            get
            {
                if (this.Any() != true)
                {
                    return EndpointManagerStatusEnum.Empty;
                }

                if (this.Any(x => x.Status.Status == EndpointStatusEnum.Initializing))
                {
                    return EndpointManagerStatusEnum.Initializing;
                }

                if (this.Any(x => x.Status.Status == EndpointStatusEnum.New))
                {
                    return EndpointManagerStatusEnum.Changed;
                }

                if (this.Any(x => x.Status.Status == EndpointStatusEnum.Failed || x.Status.Status == EndpointStatusEnum.Unhealthy) && this.Any(x =>
                    x.Status.Status != EndpointStatusEnum.Failed || x.Status.Status != EndpointStatusEnum.Unhealthy))
                {
                    return EndpointManagerStatusEnum.PartiallyRunning;
                }

                if (this.Any(x => x.Status.Status == EndpointStatusEnum.Failed))
                {
                    return EndpointManagerStatusEnum.Failed;
                }

                return EndpointManagerStatusEnum.Running;
            }
        }

        public Endpoint Create(EndpointDefinition endpointDefinition)
        {
            try
            {
                var api = _apiProvider.Get(endpointDefinition.Api);

                if (endpointDefinition.HealthCheckFactory == null)
                {
                    endpointDefinition.HealthCheckFactory = GetHealthCheckFactory(api, endpointDefinition);
                }

                var endpoint = new Endpoint(endpointDefinition, api);

                return endpoint;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create endpoint from {EndpointDefinition}", endpointDefinition);

                throw;
            }
        }

        public Endpoint CreateAndAdd(EndpointDefinition endpointDefinition)
        {
            try
            {
                var endpoint = Create(endpointDefinition);
                AddEndpoint(endpoint);

                return endpoint;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create and add endpoint from {EndpointDefinition}", endpointDefinition);

                throw;
            }
        }

        public void AddEndpoint(Endpoint endpoint)
        {
            Add(endpoint);
        }

        /// <summary>
        /// Updates the current runtime status to match the configuration. If new endpoints are added runtime, these are not applied automatically.
        /// </summary>
        public void Update()
        {
            _initializer.Initialize(this);
        }

        public void RemoveEndpoint(Endpoint endpoint)
        {
            Remove(endpoint);
            _changeNotifier.Notify();
        }

        private Func<Endpoint, Task<IHealthCheck>> GetHealthCheckFactory(Api api, EndpointDefinition endpointDefinition = null)
        {
            if (endpointDefinition?.HealthCheckFactory != null)
            {
                return endpointDefinition.HealthCheckFactory;
            }

            if (api.HealthCheckFactory != null)
            {
                return endpoint => api.HealthCheckFactory(endpoint);
            }

            IHealthCheck result = new EmptyHealthCheck();

            return endpoint => Task.FromResult(result);
        }
    }
}
