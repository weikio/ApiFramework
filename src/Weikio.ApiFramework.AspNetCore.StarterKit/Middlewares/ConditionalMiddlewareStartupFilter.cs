using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Weikio.ApiFramework.AspNetCore.StarterKit.Middlewares
{
    public class ConditionalMiddlewareStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                // wrap the builder with our interceptor
                var wrappedBuilder = new ConditionalMiddlewareBuilder(builder);
                // build the rest of the pipeline using our wrapped builder
                next(wrappedBuilder);
            };
        }
    }
}