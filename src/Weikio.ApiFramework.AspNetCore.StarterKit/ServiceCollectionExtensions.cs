using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema.Generation;
using NSwag.Generation.Processors;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.ApiFramework.Admin;
using Weikio.ApiFramework.AspNetCore.StarterKit.Middlewares;
using Weikio.ApiFramework.Core.AsyncStream;

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
                document.DefaultResponseReferenceTypeNullHandling = ReferenceTypeNullHandling.Null;
            });
            
            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework - Admin";
                document.ApiGroupNames = new[] { "api_framework_admin" };
                document.DocumentName = "Admin";
                document.DefaultResponseReferenceTypeNullHandling = ReferenceTypeNullHandling.Null;
            });
            
            services.AddOpenApiDocument(document =>
            {
                document.Title = "Api Framework - Everything";
                document.DocumentName = "All";
                document.OperationProcessors.Add(new ApiFrameworkTagOperationProcessor());
                document.DefaultResponseReferenceTypeNullHandling = ReferenceTypeNullHandling.Null;
            });

            services.AddRuntimeMiddleware();
            services.AddConditionalMiddleware();

            services.Configure<ConditionalMiddlewareOptions>("Microsoft.AspNetCore.Routing.EndpointMiddleware", opt =>
            {
                opt.Configure = appBuilder =>
                {
                    appBuilder.UseOpenApi();
                    appBuilder.UseSwaggerUi3();
                };
            });
                        
            services.Configure<MvcOptions>(options =>
            {
                options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
            });
            
            return builder;
        }
        
        // public static IApiFrameworkBuilder AddApi(this IApiFrameworkBuilder builder, string nugetPackageName, string version)
        // {
        //     builder.Services.AddTransient<IPluginCatalog>(services =>
        //     {
        //         var typeCatalog = new NugetPackagePluginCatalog(nugetPackageName, version, true);
        //
        //         return typeCatalog;
        //     });
        //
        //     return builder;
        // }
        //
        // public static IApiFrameworkBuilder AddApi(this IApiFrameworkBuilder builder, string script, string apiName, string version)
        // {
        //     builder.Services.AddTransient<IPluginCatalog>(services =>
        //     {
        //         var roslynCatalog = new RoslynPluginCatalog(script,
        //             new RoslynPluginCatalogOptions() { PluginName = apiName, PluginVersion = new Version(version) });
        //
        //         return roslynCatalog;
        //     });
        //
        //     return builder;
        // }
        //
        // public static IApiFrameworkBuilder AddApi(this IApiFrameworkBuilder builder, MulticastDelegate del, string apiName)
        // {
        //     builder.Services.AddTransient<IPluginCatalog>(services =>
        //     {
        //         var delCatalog = new DelegatePluginCatalog(del, apiName);
        //
        //         return delCatalog;
        //     });
        //
        //     return builder;
        // }
    }
}
