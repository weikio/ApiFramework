using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSwag.Generation.Processors;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.ApiFramework.Admin;
using Weikio.ApiFramework.ApiProviders.PluginFramework;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Extensions;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.ApiFramework.AspNetCore.StarterKit
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddApiFrameworkWithAdmin(this IMvcBuilder mvcBuilder, Action<ApiFrameworkAspNetCoreOptions> setupAction = null)
        {
            return mvcBuilder.Services.AddApiFramework(setupAction);
        }
        
        public static IApiFrameworkBuilder AddApiFrameworkWithAdmin(this IServiceCollection services,
            Action<ApiFrameworkAspNetCoreOptions> setupAction = null)
        {
            var builder = services.AddApiFramework();

            builder.AddAdmin();
            
            services.AddTransient<IDocumentProcessor, CustomOpenApiExtenderDocumentProcessor>();
            
            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework - APIs";
                document.DocumentName = "Api";
                document.ApiGroupNames = new[] { "api_framework_endpoint" };
                document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
            });
            
            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework - Admin";
                document.ApiGroupNames = new[] { "api_framework_admin" };
                document.DocumentName = "Admin";
            });
            
            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework - Everything";
                document.DocumentName = "All";
                document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
            });

            return builder;
        }
    }
}
