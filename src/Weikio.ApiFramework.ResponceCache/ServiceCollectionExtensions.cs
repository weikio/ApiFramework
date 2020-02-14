using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.ResponceCache
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddApiFrameworkResponseCache(this IApiFrameworkBuilder builder,
            Action<ResponceCacheOptions> setupAction = null)
        {
            var services = builder.Services;

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }
            else
            {
                services.AddSingleton<IConfigureOptions<ResponceCacheOptions>, ConfigureEndpointResponseCacheOptions>();
            }

            return builder;
        }
    }
}
