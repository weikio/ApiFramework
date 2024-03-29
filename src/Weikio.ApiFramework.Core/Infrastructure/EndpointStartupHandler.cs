﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.StartupTasks;
using Weikio.ApiFramework.SDK;
using Weikio.AspNetCore.Common;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    /// <summary>
    /// Startup task which initializes all the endpoints when run. The task isn't automatically run, usually it is started by <see cref="ApiProviderInitializer"/>.
    /// TODO: This shouldn't be a startup task. Maybe interface & implementation which can be called when needed. The question is if this should initialize all the plugins as they can take a long time.
    /// </summary>
    public class EndpointStartupHandler : IEndpointStartupHandler
    {
        private readonly EndpointConfigurationManager _endpointConfigurationManager;
        private readonly IEndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly ApiFrameworkOptions _options;

        public EndpointStartupHandler(IEndpointManager endpointManager,
            EndpointConfigurationManager endpointConfigurationManager, IApiProvider apiProvider, IOptions<ApiFrameworkOptions> options,
            IBackgroundTaskQueue backgroundTaskQueue)
        {
            _endpointManager = endpointManager;
            _endpointConfigurationManager = endpointConfigurationManager;
            _apiProvider = apiProvider;
            _backgroundTaskQueue = backgroundTaskQueue;
            _options = options.Value;
        }

        public void Start(CancellationToken cancellationToken)
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(async x =>
            {
                var initialEndpoints = await _endpointConfigurationManager.GetEndpointDefinitions();

                var endpointsAdded = false;

                foreach (var endpointDefinition in initialEndpoints)
                {
                    _endpointManager.CreateAndAdd(endpointDefinition);
                    endpointsAdded = true;
                }

                if (initialEndpoints.Any() == false && _options.AutoResolveEndpoints)
                {
                    var apis = _apiProvider.List();

                    foreach (var apiDefinition in apis)
                    {
                        var endpointDefinition = new EndpointDefinition(apiDefinition.Name.Replace(".","").Replace(" ", "").Trim(), apiDefinition);
                        _endpointManager.CreateAndAdd(endpointDefinition);

                        endpointsAdded = true;
                    }
                }

                if (!endpointsAdded)
                {
                    return;
                }

                _endpointManager.Update();
            });
        }
    }
}
