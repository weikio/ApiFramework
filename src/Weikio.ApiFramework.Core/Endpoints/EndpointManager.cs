using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.HealthChecks;

namespace Weikio.ApiFramework.Core.Endpoints
{
    public static class IEndpointManagerExtensions
    {
        public static Endpoint Create(this IEndpointManager endpointManager, string route, Api api, object configuration)
        {
            var def = new EndpointDefinition(route, api.ApiDefinition, configuration);
            var result = endpointManager.Create(def);

            return result;
        }
        public static Endpoint CreateAndAdd(this IEndpointManager endpointManager, string route, Api api, object configuration)
        {
            var def = new EndpointDefinition(route, api.ApiDefinition, configuration);
            var result = endpointManager.CreateAndAdd(def);

            return result;
        }
    }

    public interface IEndpointManager
    {
        EndpointManagerStatusEnum Status { get; }
        List<Endpoint> Endpoints { get; }
        Endpoint Create(EndpointDefinition endpointDefinition);
        Endpoint CreateAndAdd(EndpointDefinition endpointDefinition);
        void AddEndpoint(Endpoint endpoint);

        /// <summary>
        /// Updates the current runtime status to match the configuration. If new endpoints are added runtime, these are not applied automatically.
        /// </summary>
        void Update();

        void RemoveEndpoint(Endpoint endpoint);
    }

    public class DefaultEndpointManager : List<Endpoint>, IEndpointManager
    {
        private readonly IEndpointInitializer _initializer;
        private readonly IApiProvider _apiProvider;
        private readonly ILogger<DefaultEndpointManager> _logger;

        public List<Endpoint> Endpoints
        {
            get
            {
                return this;
            }
        }

        public DefaultEndpointManager(IEndpointInitializer initializer, IApiProvider apiProvider, ILogger<DefaultEndpointManager> logger)
        {
            _initializer = initializer;
            _apiProvider = apiProvider;
            _logger = logger;
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
                var healthCheckFactory = GetHealthCheckFactory(api, endpointDefinition);
                var endpoint = new Endpoint(endpointDefinition, api, healthCheckFactory);

                return endpoint;
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Failed to create endpoint from {EndpointDefinition}", endpointDefinition);

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
                _logger.LogError(e,"Failed to create and add endpoint from {EndpointDefinition}", endpointDefinition);

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
        }

        private Func<Endpoint, Task<IHealthCheck>> GetHealthCheckFactory(Api api, EndpointDefinition endpointDefinition = null)
        {
            if (endpointDefinition?.HealthCheck != null)
            {
                return endpoint => Task.FromResult(endpoint.HealthCheck);
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
