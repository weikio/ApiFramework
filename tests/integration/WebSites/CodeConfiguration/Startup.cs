using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.AspNetCore;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.ApiFramework.Core.StartupTasks;
using Weikio.ApiFramework.SDK;
using Weikio.AspNetCore.Common;

namespace CodeConfiguration
{
    public class SyncEndpointInitializer : IEndpointInitializer
    {
        private readonly ILogger<EndpointInitializer> _logger;
        private readonly ApiChangeNotifier _changeNotifier;
        private readonly ApiFrameworkOptions _options;

        public SyncEndpointInitializer(ILogger<EndpointInitializer> logger, ApiChangeNotifier changeNotifier, IOptions<ApiFrameworkOptions> options)
        {
            _logger = logger;
            _changeNotifier = changeNotifier;
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

            foreach (var endpoint in endpoints)
            {
                Initialize(endpoint, force).Wait();
            }
            
            if (_options.ChangeNotificationType == ChangeNotificationTypeEnum.Batch)
            {
                _changeNotifier.Notify();
            }
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

    public class SyncApiProviderInitializer : IApiProviderInitializer
    {
        private readonly ILogger<ApiProviderInitializer> _logger;
        private readonly IApiProvider _apiProvider;
        private readonly IEndpointStartupHandler _endpointStartupHandler;

        public SyncApiProviderInitializer(ILogger<ApiProviderInitializer> logger, IApiProvider apiProvider, IEndpointStartupHandler endpointStartupHandler)
        {
            _logger = logger;
            _apiProvider = apiProvider;
            _endpointStartupHandler = endpointStartupHandler;
        }

        public void Initialize()
        {
            _apiProvider.Initialize(new CancellationToken()).Wait();

            var allApis = _apiProvider.List().Result;

            _logger.LogDebug($"There's {allApis.Count} apis available:");
            
            _endpointStartupHandler.Start(new CancellationToken());
        }
    }

    public class SyncEndpointStartupHandler : IEndpointStartupHandler
    {
        private readonly EndpointConfigurationManager _endpointConfigurationManager;
        private readonly EndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;
        private readonly ApiFrameworkOptions _options;

        public SyncEndpointStartupHandler(EndpointManager endpointManager,
            EndpointConfigurationManager endpointConfigurationManager, IApiProvider apiProvider, IOptions<ApiFrameworkOptions> options)
        {
            _endpointManager = endpointManager;
            _endpointConfigurationManager = endpointConfigurationManager;
            _apiProvider = apiProvider;
            _options = options.Value;
        }

        public void Start(CancellationToken cancellationToken)
        {
            var initialEndpoints = _endpointConfigurationManager.GetEndpointDefinitions().Result;

            var endpointsAdded = false;

            foreach (var endpointDefinition in initialEndpoints)
            {
                var api = _apiProvider.Get(endpointDefinition.Api).Result;

                var endpoint = new Endpoint(endpointDefinition.Route, api, endpointDefinition.Configuration,
                    GetHealthCheckFactory(api, endpointDefinition));

                _endpointManager.AddEndpoint(endpoint);
                endpointsAdded = true;
            }

            if (initialEndpoints.Any() == false && _options.AutoResolveEndpoints)
            {
                var functions = _apiProvider.List().Result;

                foreach (var functionDefinition in functions)
                {
                    var function = _apiProvider.Get(functionDefinition).Result;
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

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddSingleton<IApiProviderInitializer, SyncApiProviderInitializer>();
            services.AddSingleton<IEndpointStartupHandler, SyncEndpointStartupHandler>();
            services.AddSingleton<IEndpointInitializer, SyncEndpointInitializer>();

            services.AddSwaggerDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
