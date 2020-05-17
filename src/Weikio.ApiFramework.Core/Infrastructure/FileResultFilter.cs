using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Core.Configuration;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class FileResultFilter : IAsyncResultFilter
    {
        private static readonly ConcurrentDictionary<string, IFileStreamResultConverter> _memoryCache = new ConcurrentDictionary<string, IFileStreamResultConverter>();
        private readonly ILogger<FileResultFilter> _logger;
        private readonly IEnumerable<IFileStreamResultConverter> _fileResultConverters;
        private readonly ApiFrameworkOptions _options;

        public FileResultFilter(ILogger<FileResultFilter> logger, IEnumerable<IFileStreamResultConverter> fileResultConverters, IOptions<ApiFrameworkOptions> options)
        {
            _logger = logger;
            _fileResultConverters = fileResultConverters;
            _options = options.Value;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            try
            {
                if (_options.AutoConvertFileToStream == false)
                {
                    _logger.LogTrace("Auto conversion from file to stream disabled. Skip filter.");
                    return;
                }

                if (context.Result is ObjectResult objResult)
                {
                    if (_fileResultConverters?.Any() != true)
                    {
                        _logger.LogDebug("No IFileResultConverter implementations found from DI. Skip filter.");
                        return;
                    }

                    if (objResult.DeclaredType == null)
                    {
                        return;
                    }

                    var converter = _memoryCache.GetOrAdd(objResult.DeclaredType.FullName, entry =>
                    {
                        foreach (var availableConverter in _fileResultConverters)
                        {
                            var canConvert = availableConverter.CanConvertType(objResult.DeclaredType);

                            if (canConvert)
                            {
                                return availableConverter;
                            }

                            _logger.LogTrace("{FileResultConverter} can not handle {Type}. Trying the next IFileResultConverter.", availableConverter.GetType().Name, objResult.DeclaredType.Name);
                        }

                        return null;
                    });

                    if (converter == null)
                    {
                        _logger.LogDebug("IFileResultConverter not available for converting {Type}", objResult.DeclaredType.FullName);
                
                        return;  
                    }
                    
                    _logger.LogDebug("Handling file result conversion of {Type} with {FileResultConverter}.", objResult.DeclaredType.Name, converter.GetType().Name );
                    context.Result = await converter.Convert(objResult.Value);
                }
            }
            finally
            {
                await next();
            }
        }
    }
}
