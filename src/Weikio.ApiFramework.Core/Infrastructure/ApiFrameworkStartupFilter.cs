using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ApiFrameworkStartupFilter : IStartupFilter
    {
        private readonly ApiFeatureProvider _apiFeatureProvider;
        private readonly ApplicationPartManager _applicationPartManager;

        public ApiFrameworkStartupFilter(ApiFeatureProvider apiFeatureProvider, ApplicationPartManager applicationPartManager)
        {
            _apiFeatureProvider = apiFeatureProvider;
            _applicationPartManager = applicationPartManager;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            _applicationPartManager.FeatureProviders.Add(_apiFeatureProvider);

            return next;
        }
    }
}
