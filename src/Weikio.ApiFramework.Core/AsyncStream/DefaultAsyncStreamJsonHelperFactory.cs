using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Weikio.ApiFramework.Core.AsyncStream
{
    internal class DefaultAsyncStreamJsonHelperFactory : IAsyncStreamJsonHelperFactory
    {
        private readonly IActionResultExecutor<JsonResult> _jsonResultSerializer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DefaultAsyncStreamJsonHelperFactory> _logger;

        public DefaultAsyncStreamJsonHelperFactory(IActionResultExecutor<JsonResult> jsonResultSerializer, IServiceProvider serviceProvider,
            ILogger<DefaultAsyncStreamJsonHelperFactory> logger)
        {
            _jsonResultSerializer = jsonResultSerializer;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public IAsyncStreamJsonHelper Create()
        {
            if (_jsonResultSerializer.GetType().Assembly == typeof(JsonResult).Assembly)
            {
                _logger.LogTrace("Using System.Text.Json for handling streaming output.");

                // Use System.Text.Json
                return _serviceProvider.GetRequiredService<SystemTextAsyncStreamJsonHelper>();
            }

            // Use NewtonSoft.Json. Others are not supported at this point
            _logger.LogTrace("Using Newtonsoft Json for handling streaming output.");

            return _serviceProvider.GetRequiredService<NewtonsoftAsyncStreamJsonHelper>();
        }
    }
}
