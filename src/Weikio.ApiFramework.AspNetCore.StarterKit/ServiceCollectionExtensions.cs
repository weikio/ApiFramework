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

namespace Weikio.ApiFramework.AspNetCore.StarterKit
{
    public static class ServiceCollectionExtensions
    {
        public static IApiFrameworkBuilder AddApiFrameworkStarterKit(this IMvcBuilder mvcBuilder, Action<ApiFrameworkAspNetCoreOptions> setupAction = null)
        {
            return mvcBuilder.Services.AddApiFramework(setupAction);
        }
        
        public static IApiFrameworkBuilder AddApiFrameworkStarterKit(this IServiceCollection services,
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

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }
            
            return builder;
        }
    }
}
