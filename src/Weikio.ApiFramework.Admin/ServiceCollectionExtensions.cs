using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.AspNetCore.Common;

namespace Weikio.ApiFramework.Admin
{
    public class ApiFrameworkAdminOptions
    {
        public string EndpointApiPolicy = "";
        public string EndpointAdminRouteRoot = "admin/api";
    }

    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddAdmin(this IApiFrameworkBuilder builder,
            Action<ApiFrameworkAdminOptions> setupAction = null)
        {
            builder.Services.AddSingleton(provider =>
            {
                var configurationOptions = provider.GetService<IOptions<ApiFrameworkAdminOptions>>();

                var configuration = configurationOptions.Value;

                if (string.IsNullOrWhiteSpace(configuration.EndpointApiPolicy))
                {
                    return null;
                }

                return new ApiFrameworkAdminActionConvention(configuration.EndpointApiPolicy);
            });

            builder.Services.AddSingleton(provider =>
            {
                var configurationOptions = provider.GetService<IOptions<ApiFrameworkAdminOptions>>();

                var configuration = configurationOptions.Value;

                if (string.IsNullOrWhiteSpace(configuration.EndpointAdminRouteRoot))
                {
                    return null;
                }

                return new NamespaceRoutingConvention(configuration.EndpointAdminRouteRoot);
            });
            
            builder.Services.ConfigureWithDependencies<MvcOptions, ApiFrameworkAdminActionConvention>((mvcOptions, convention) =>
            {
                if (convention == null)
                {
                    return;
                }
                
                mvcOptions.Conventions.Add(convention);
            });
            
            builder.Services.ConfigureWithDependencies<MvcOptions, NamespaceRoutingConvention>((mvcOptions, convention) =>
            {
                if (convention == null)
                {
                    return;
                }
                
                mvcOptions.Conventions.Add(convention);
            });
            
            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }
            else
            {
                builder.Services.Configure<ApiFrameworkAdminOptions>(defaultOptions =>
                {
                    defaultOptions.EndpointApiPolicy = "";
                });
            }

            return builder;
        }
    }
}
