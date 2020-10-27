using System;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Abstractions.DependencyInjection;

namespace Weikio.ApiFramework.OData
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddOData(this IApiFrameworkBuilder builder,
            Action<ODataOptions> setupAction = null)
        {
            var services = builder.Services;

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            // builder.Services.AddOData(options =>
            //     {
            //         options.MaxTop = 500;
            //     }
            // );

            return builder;
        }
    }

    public class ODataOptions
    {
    }
}
