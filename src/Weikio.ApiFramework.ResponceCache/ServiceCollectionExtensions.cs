using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.ApiFramework.ResponceCache;

// ReSharper disable once CheckNamespace
namespace Weikio.ApiFramework
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
