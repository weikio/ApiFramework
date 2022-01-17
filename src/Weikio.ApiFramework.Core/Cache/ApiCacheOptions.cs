using System;
using System.Collections.Generic;
using System.Text;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Core.Cache
{
    public class ApiCacheOptions
    {
        public TimeSpan ExpirationTime { get; set; } = TimeSpan.FromSeconds(60);
        public Func<Endpoint, IServiceProvider, string, string> GetKey { get; set; } = (endpoint, provider, key) =>
        {
            var route = endpoint.Route;
            var result = $"{route}{key}";

            return result;
        };
    }
}
