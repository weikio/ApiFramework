using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Weikio.ApiFramework.AspNetCore.StarterKit.Middlewares
{
    // Thanks to https://stackoverflow.com/a/56751443/66988
    public class RuntimeMiddlewareService
    {
        private readonly IOptionsMonitor<ConditionalMiddlewareOptions> _optionsSnapshot;
        private IApplicationBuilder _appBuilder;
        private Dictionary<string, Func<RequestDelegate, RequestDelegate>> _middlewares = new Dictionary<string, Func<RequestDelegate, RequestDelegate>>();
        private static string _buildLock = "lock";
        
        public RuntimeMiddlewareService(IOptionsMonitor<ConditionalMiddlewareOptions> optionsSnapshot)
        {
            _optionsSnapshot = optionsSnapshot;
        }

        internal void Use(IApplicationBuilder app)
        {
            _appBuilder = app.Use(next =>
            {
                return httpContext =>
                {
                    if (next.Target != null)
                    {
                        var key = next.Target.GetType().FullName;

                        if (string.IsNullOrWhiteSpace(key))
                        {
                            return next(httpContext);
                        }
                        
                        if (_middlewares.ContainsKey(key))
                        {
                            var middleware = _middlewares[key];

                            return middleware(next)(httpContext);
                        }

                        lock (_buildLock)
                        {
                            if (_middlewares.ContainsKey(key))
                            {
                                var middleware = _middlewares[key];

                                return middleware(next)(httpContext);
                            }
                            
                            var createdMiddleware = BuildMiddleware(key);
                            _middlewares.Add(key, createdMiddleware);

                            return createdMiddleware(next)(httpContext);
                        }
                    }

                    return next(httpContext);
                };
            });
        }

        private Func<RequestDelegate, RequestDelegate> BuildMiddleware(string beforeMiddleware)
        {
            var options = _optionsSnapshot.Get(beforeMiddleware);

            if (options.Configure == null)
            {
                options = _optionsSnapshot.Get(Options.DefaultName);
            }
            
            var app = _appBuilder.New();
            
            options.Configure(app);
            
            Func<RequestDelegate, RequestDelegate> result = next =>
            {
                return app.Use(_ => next).Build();
            };

            return result;
        }
    }
}
