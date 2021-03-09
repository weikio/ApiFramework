using System;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class DefaultEndpointRouteTemplateProvider : IEndpointRouteTemplateProvider
    {
        private readonly ApiFrameworkOptions _options;

        public DefaultEndpointRouteTemplateProvider(IOptions<ApiFrameworkOptions> options)
        {
            _options = options.Value;
        }

        public string GetRouteTemplate(Endpoint endpoint)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            var endpointRoute = endpoint.Route;
            
            if (string.IsNullOrWhiteSpace(endpointRoute))
            {
                throw new ArgumentNullException(nameof(endpointRoute));
            }

            var apiAddressBase = GetApiAddressBase();
            if (string.IsNullOrWhiteSpace(apiAddressBase))
            {
                return endpointRoute;
            }

            return apiAddressBase + endpointRoute.TrimStart('/').TrimEnd('/').Trim();
        }

        public string GetApiAddressBase()
        {
            if (string.IsNullOrWhiteSpace(_options.ApiAddressBase))
            {
                return string.Empty;
            }
            
            if (_options.ApiAddressBase.EndsWith('/'))
            {
                return _options.ApiAddressBase;
            }

            return _options.ApiAddressBase + '/';
        }
    }
}
