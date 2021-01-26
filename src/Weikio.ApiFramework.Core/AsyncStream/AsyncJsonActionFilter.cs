using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Weikio.ApiFramework.Core.AsyncStream
{
    // Functionality for handling large amounts of json. Sends response in parts, using memory based buffering
    // From GitHub issue' comments: https://github.com/dotnet/runtime/issues/1570#issuecomment-676594141
    internal class AsyncJsonActionFilter : IAsyncActionFilter
    {
        private readonly IAsyncStreamJsonHelperFactory _jsonHelperFactory;
        private readonly ILogger<AsyncJsonActionFilter> _logger;
        private readonly AsyncStreamJsonOptions _options;

        public AsyncJsonActionFilter(IOptions<AsyncStreamJsonOptions> options, IAsyncStreamJsonHelperFactory jsonHelperFactory,
            ILogger<AsyncJsonActionFilter> logger)
        {
            _jsonHelperFactory = jsonHelperFactory;
            _logger = logger;
            _options = options.Value;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (_options.IsEnabled == false)
            {
                _logger.LogTrace("Async Json Stream handling is not enabled, skipping.");
                await next();

                return;
            }

            _logger.LogTrace("Async Json Stream handling is enabled");

            var action = context.ActionDescriptor as ControllerActionDescriptor;
            var returnType = action?.MethodInfo?.ReturnType;

            if (returnType is null || !returnType.IsGenericType || returnType.GetGenericTypeDefinition() != typeof(IAsyncEnumerable<>))
            {
                _logger.LogTrace("Return type is {ReturnType}, only 'IAsyncEnumerable<>' is supported, skipping.", returnType);

                await next();

                return;
            }

            _logger.LogTrace("Action's {Action} return type is {ReturnType}, automatically convert it to async json stream.", action.DisplayName, returnType);

            var parameters = action.MethodInfo.GetParameters()
                .Select(x =>
                {
                    if (context.ActionArguments.TryGetValue(x.Name ?? string.Empty, out var p))
                    {
                        return p;
                    }

                    if (x.ParameterType.GetTypeInfo().IsValueType)
                    {
                        return Activator.CreateInstance(x.ParameterType);
                    }

                    return null;
                }).ToArray();

            var result = action.MethodInfo.Invoke(context.Controller, parameters);

            var method = typeof(AsyncJsonActionFilter)
                .GetMethod(nameof(WriteAsyncStream),
                    BindingFlags.NonPublic | BindingFlags.Instance);

            if (method is { })
            {
                var generic = method.MakeGenericMethod(returnType.GetGenericArguments().First());
                var task = (Task) generic.Invoke(this, new[] { context.HttpContext.Response, result, context.HttpContext });

                if (task != null && task.IsCompleted == false)
                {
                    await task;
                }
            }
            else
            {
                await next();
            }
        }

        private async Task WriteAsyncStream<T>(HttpResponse response, IAsyncEnumerable<T> stream, HttpContext context)
        {
            response.StatusCode = (int) HttpStatusCode.OK;
            response.ContentType = $"{MediaTypeNames.Application.Json};charset={Encoding.UTF8.WebName}";

            await using var buffer = new MemoryStream();
            var writer = _jsonHelperFactory.Create();

            try
            {
                writer.Initialize(buffer);

                writer.WriteStartArray();

                // Todo: Add support for cancellation token using EnumeratorCancellation
                await foreach (var item in stream)
                {
                    writer.Serialize(item);

                    if (buffer.Length < _options.BufferSizeThreshold)
                    {
                        continue;
                    }

                    // Write to response stream
                    _logger.LogTrace("Buffer size {BufferSize} exceeded buffer threshold {BufferThreshold}, write to response body and continue.",
                        buffer.Length, _options.BufferSizeThreshold);
                    buffer.Position = 0;
                    await buffer.CopyToAsync(response.Body, context.RequestAborted);

                    buffer.Position = 0;
                    buffer.SetLength(0);
                }

                writer.WriteEndArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to write async stream to response stream.");
            }
            finally
            {
                await writer.DisposeAsync().ConfigureAwait(false);
                buffer.Position = 0;
                await buffer.CopyToAsync(response.Body, context.RequestAborted);

                _logger.LogTrace("Async stream completed, closing the response body.");

                response.Body.Close(); 
            }
        }
    }
}
