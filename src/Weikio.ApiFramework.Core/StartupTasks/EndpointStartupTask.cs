using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.AspNetCore.StartupTasks;

namespace Weikio.ApiFramework.Core.StartupTasks
{
    /// <summary>
    /// Startup task which initializes all the endpoints when run. The task isn't automatically run, usually it is started by <see cref="ApiDefinitionsStartupTask"/>.
    /// TODO: This shouldn't be a startup task. Maybe interface & implementation which can be called when needed. The question is if this should initialize all the plugins as they can take a long time.
    /// </summary>
    public class EndpointStartupTask : IStartupTask
    {
        private readonly EndpointConfigurationManager _endpointConfigurationManager;
        private readonly EndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;
        private readonly ApiFrameworkOptions _options;

        public EndpointStartupTask(EndpointManager endpointManager,
            EndpointConfigurationManager endpointConfigurationManager, IApiProvider apiProvider, IOptions<ApiFrameworkOptions> options)
        {
            _endpointManager = endpointManager;
            _endpointConfigurationManager = endpointConfigurationManager;
            _apiProvider = apiProvider;
            _options = options.Value;
        }

        public bool IsAutomatic
        {
            get
            {
                return false;
            }
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var initialEndpoints = await _endpointConfigurationManager.GetEndpointConfigurations();

            var endpointsAdded = false;

            foreach (var endpointConfiguration in initialEndpoints)
            {
                var function = await _apiProvider.Get(endpointConfiguration.Api);

                var endpoint = new Endpoint(endpointConfiguration.Route, function, endpointConfiguration.Configuration,
                    GetHealthCheckFactory(function, endpointConfiguration));

                _endpointManager.AddEndpoint(endpoint);
                endpointsAdded = true;
            }

            if (initialEndpoints.Any() == false && _options.AutoResolveEndpoints)
            {
                var functions = await _apiProvider.List();

                foreach (var functionDefinition in functions)
                {
                    var function = await _apiProvider.Get(functionDefinition);
                    var endpoint = new Endpoint(_options.ApiAddressBase, function, null, GetHealthCheckFactory(function));

                    _endpointManager.AddEndpoint(endpoint);

                    endpointsAdded = true;
                }
            }

            if (!endpointsAdded)
            {
                return;
            }

            _endpointManager.Update();
        }

        private Func<Endpoint, Task<IHealthCheck>> GetHealthCheckFactory(Api api, EndpointConfiguration endpointConfiguration = null)
        {
            if (endpointConfiguration?.HealthCheck != null)
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
