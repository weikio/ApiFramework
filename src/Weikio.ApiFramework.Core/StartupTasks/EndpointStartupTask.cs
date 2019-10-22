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
    /// Startup task which initializes all the endpoints when run. The task isn't automatically run, usually it is started by <see cref="FunctionDefinitionsStartupTask"/>.
    /// TODO: This shouldn't be a startup task. Maybe interface & implementation which can be called when needed. The question is if this should initialize all the plugins as they can take a long time.
    /// </summary>
    public class EndpointStartupTask : IStartupTask
    {
        private readonly EndpointConfigurationManager _endpointConfigurationManager;
        private readonly EndpointManager _endpointManager;
        private readonly IFunctionProvider _functionProvider;
        private readonly FunctionFrameworkOptions _options;

        public EndpointStartupTask(EndpointManager endpointManager,
            EndpointConfigurationManager endpointConfigurationManager, IFunctionProvider functionProvider, IOptions<FunctionFrameworkOptions> options)
        {
            _endpointManager = endpointManager;
            _endpointConfigurationManager = endpointConfigurationManager;
            _functionProvider = functionProvider;
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
                var function = await _functionProvider.Get(endpointConfiguration.Function);

                var endpoint = new Endpoint(endpointConfiguration.Route, function, endpointConfiguration.Configuration,
                    GetHealthCheckFactory(function, endpointConfiguration));

                _endpointManager.AddEndpoint(endpoint);
                endpointsAdded = true;
            }

            if (initialEndpoints.Any() == false && _options.AutoResolveEndpoints)
            {
                var functions = await _functionProvider.List();

                foreach (var functionDefinition in functions)
                {
                    var function = await _functionProvider.Get(functionDefinition);
                    var endpoint = new Endpoint(_options.FunctionAddressBase, function, null, GetHealthCheckFactory(function));

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

        private Func<Endpoint, Task<IHealthCheck>> GetHealthCheckFactory(Function function, EndpointConfiguration endpointConfiguration = null)
        {
            if (endpointConfiguration?.HealthCheck != null)
            {
                return endpoint => Task.FromResult(endpoint.HealthCheck);
            }

            if (function.HealthCheckFactory != null)
            {
                return endpoint => function.HealthCheckFactory(endpoint);
            }

            IHealthCheck result = new EmptyHealthCheck();

            return endpoint => Task.FromResult(result);
        }
    }
}
