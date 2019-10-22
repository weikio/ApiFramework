using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class FunctionFrameworkStartupFilter : IStartupFilter
    {
        private readonly FunctionFeatureProvider _functionFeatureProvider;
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly EndpointManager _endpointManager;
        private readonly EndpointConfigurationManager _endpointConfigurationManager;

        public FunctionFrameworkStartupFilter(FunctionFeatureProvider functionFeatureProvider, ApplicationPartManager applicationPartManager,
            EndpointManager endpointManager, EndpointConfigurationManager endpointConfigurationManager)
        {
            _functionFeatureProvider = functionFeatureProvider;
            _applicationPartManager = applicationPartManager;
            _endpointManager = endpointManager;
            _endpointConfigurationManager = endpointConfigurationManager;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            _applicationPartManager.FeatureProviders.Add(_functionFeatureProvider);

//
//            _pluginManager.InstallPlugins(new List<string>() { "C:\\dev\\projects\\Weik.io\\src\\FunctionFramework\\samples\\JsonConfiguration\\bin\\Debug\\netcoreapp2.2\\Weikio.ApiFramework.Plugins.HelloWorld.dll" });
//            _pluginManager.InstallPlugins(new List<string>() { "C:\\dev\\projects\\Weik.io\\src\\FunctionFramework\\samples\\JsonConfiguration\\bin\\Debug\\netcoreapp2.2\\FunctionFramework.Plugins.VertexFlow.dll" });
//
//            var initialEndpoints = _endpointConfigurationManager.GetEndpointConfigurations();
//            foreach (var endpointConfiguration in initialEndpoints)
//            {
//                var function = _functionCatalog.GetByName(endpointConfiguration.Function);
//
//                var endpoint = new Endpoint(endpointConfiguration.Route, function, endpointConfiguration.Configuration);
//                endpoint.Initialize(null);
//
//                _endpointManager.AddEndpoint(endpoint);
//            }

            return next;
        }
    }
}
