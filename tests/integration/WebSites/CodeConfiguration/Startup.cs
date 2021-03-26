using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.ApiFramework.Core.Infrastructure;
using Weikio.ApiFramework.Core.StartupTasks;

namespace CodeConfiguration
{
    public class SyncEndpointInitializer : IEndpointInitializer
    {
        private readonly ILogger<EndpointInitializer> _logger;
        private readonly ApiChangeNotifier _changeNotifier;
        private readonly IApiConfigurationTypeProvider _apiConfigurationTypeProvider;
        private readonly ApiFrameworkOptions _options;

        public SyncEndpointInitializer(ILogger<EndpointInitializer> logger, ApiChangeNotifier changeNotifier, IOptions<ApiFrameworkOptions> options, IApiConfigurationTypeProvider apiConfigurationTypeProvider)
        {
            _logger = logger;
            _changeNotifier = changeNotifier;
            _apiConfigurationTypeProvider = apiConfigurationTypeProvider;
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

    public class SyncApiProviderInitializer : IApiProviderInitializer
    {
        private readonly ILogger<SyncApiProviderInitializer> _logger;
        private readonly IApiProvider _apiProvider;
        private readonly IEndpointStartupHandler _endpointStartupHandler;

        public SyncApiProviderInitializer(ILogger<SyncApiProviderInitializer> logger, IApiProvider apiProvider, IEndpointStartupHandler endpointStartupHandler)
        {
            _logger = logger;
            _apiProvider = apiProvider;
            _endpointStartupHandler = endpointStartupHandler;
        }

        public async Task Initialize()
        {
            await _apiProvider.Initialize(new CancellationToken());

            var allApis = _apiProvider.List();

            _logger.LogDebug($"There's {allApis.Count} apis available:");
            
            _endpointStartupHandler.Start(new CancellationToken());
        }
    }

    public class SyncEndpointStartupHandler : IEndpointStartupHandler
    {
        private readonly EndpointConfigurationManager _endpointConfigurationManager;
        private readonly IEndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;
        private readonly ApiFrameworkOptions _options;

        public SyncEndpointStartupHandler(IEndpointManager endpointManager,
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
                _endpointManager.CreateAndAdd(endpointDefinition);
                endpointsAdded = true;
            }

            if (initialEndpoints.Any() == false && _options.AutoResolveEndpoints)
            {
                var apis = _apiProvider.List();

                foreach (var apiDefinition in apis)
                {
                    var def = new EndpointDefinition(_options.ApiAddressBase, apiDefinition);
                    _endpointManager.CreateAndAdd(def);

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

            services.AddControllers();

            services.AddSingleton<IApiProviderInitializer, SyncApiProviderInitializer>();
            services.AddSingleton<IEndpointStartupHandler, SyncEndpointStartupHandler>();
            services.AddSingleton<IEndpointInitializer, SyncEndpointInitializer>();

            services.AddSwaggerDocument();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "BasicAuthentication";
                    options.DefaultChallengeScheme = "BasicAuthentication";
                })
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                
            services.AddAuthorization(options =>
            {
                options.AddPolicy("testPolicy", policy);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
