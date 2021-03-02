using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.StartupTasks;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ApiFrameworkStartupFilter : IHostedService
    {
        private readonly ApiFeatureProvider _apiFeatureProvider;
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IApiProviderInitializer _apiProviderInitializer;
        private ApiFrameworkOptions _options;

        public ApiFrameworkStartupFilter(ApiFeatureProvider apiFeatureProvider, ApplicationPartManager applicationPartManager, IOptions<ApiFrameworkOptions> options, IApiProviderInitializer apiProviderInitializer)
        {
            _apiFeatureProvider = apiFeatureProvider;
            _applicationPartManager = applicationPartManager;
            _apiProviderInitializer = apiProviderInitializer;
            _options = options.Value;
        }
    
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            _applicationPartManager.FeatureProviders.Add(_apiFeatureProvider);

            if (_options.AutoInitializeApiProvider)
            {
                _apiProviderInitializer.Initialize();
            }
            
            return next;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationPartManager.FeatureProviders.Add(_apiFeatureProvider);

            if (_options.AutoInitializeApiProvider)
            {
               await _apiProviderInitializer.Initialize();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
