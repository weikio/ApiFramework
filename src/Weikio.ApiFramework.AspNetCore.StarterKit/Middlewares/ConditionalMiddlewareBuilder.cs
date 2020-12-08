using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Weikio.ApiFramework.AspNetCore.StarterKit.Middlewares
{
    // Thanks to https://andrewlock.net/inserting-middleware-between-userouting-and-useendpoints-as-a-library-author-part-1/
    internal class ConditionalMiddlewareBuilder : IApplicationBuilder
    {
        public ConditionalMiddlewareBuilder(IApplicationBuilder inner)
        {
            InnerBuilder = inner;
        }

        private IApplicationBuilder InnerBuilder { get; }

        public IServiceProvider ApplicationServices
        {
            get => InnerBuilder.ApplicationServices;
            set => InnerBuilder.ApplicationServices = value;
        }

        public IDictionary<string, object> Properties => InnerBuilder.Properties;
        public IFeatureCollection ServerFeatures => InnerBuilder.ServerFeatures;
        public RequestDelegate Build() => InnerBuilder.Build();
        public IApplicationBuilder New() => throw new NotImplementedException();

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            // Add the runtime middleware before each other middleware
            return InnerBuilder
                .UseRuntimeMiddleware()
                .Use(middleware);
        }
    }

}
