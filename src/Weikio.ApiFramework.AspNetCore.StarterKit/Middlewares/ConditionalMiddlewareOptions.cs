using System;
using Microsoft.AspNetCore.Builder;

namespace Weikio.ApiFramework.AspNetCore.StarterKit.Middlewares
{
    public class ConditionalMiddlewareOptions
    {
        public Action<IApplicationBuilder> Configure { get; set; } = null;
    }
}