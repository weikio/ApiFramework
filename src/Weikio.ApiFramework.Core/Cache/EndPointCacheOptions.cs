using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Weikio.ApiFramework.AspNetCore;

namespace Weikio.ApiFramework.Core.Cache
{
    public class EndPointCacheOptions : ICacheOptions
    {
        private readonly Abstractions.Endpoint _endPoint;
        private readonly IServiceProvider _serviceProvider;

        public EndPointCacheOptions(IHttpContextAccessor accessor, IServiceProvider serviceProvider)
        {
            _endPoint = accessor.HttpContext.GetEndpointMetadata();
            _serviceProvider = serviceProvider;
        }

        public Func<string, string> KeyFunc {
            get 
            {
                return GetKey;
            } 
        }

        public string GetKey(string key)
        {
            return $"{_endPoint.Route}{key}";
        }
    }
}
