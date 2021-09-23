using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;

namespace Weikio.ApiFramework.Core.Infrastructure
{
    public class ApiFileResponseTypeDescriptor : IApiDescriptionProvider
    {
        private readonly ILogger<ApiFileResponseTypeDescriptor> _logger;
        private readonly IEnumerable<IFileStreamResultConverter> _fileResultConverters;
        private readonly ApiFrameworkOptions _options;

        public ApiFileResponseTypeDescriptor(IOptions<ApiFrameworkOptions> options, ILogger<ApiFileResponseTypeDescriptor> logger,
            IEnumerable<IFileStreamResultConverter> fileResultConverters)
        {
            _logger = logger;
            _fileResultConverters = fileResultConverters;
            _options = options.Value;
        }

        public int Order => 1000;

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            if (_options.AutoConvertFileToStream == false)
            {
                _logger.LogTrace("Auto conversion from file to stream disabled. Skip Api Description change.");

                return;
            }

            if (_fileResultConverters?.Any() != true)
            {
                _logger.LogDebug("No IFileResultConverter implementations found from DI. Skip Api Description change.");

                return;
            }

            foreach (var apiDescription in context.Results)
            {
                var endpointMetadata = apiDescription.ActionDescriptor.EndpointMetadata?.OfType<Endpoint>().FirstOrDefault();

                // Only apply the possible conversion to Api Framework's endpoints
                if (endpointMetadata == null)
                {
                    continue;
                }

                foreach (var responseType in apiDescription?.SupportedResponseTypes)
                {
                    // No need to cache these as the logic is only run once when the API description is generated

                    foreach (var converter in _fileResultConverters)
                    {
                        var canConvert = converter.CanConvertType(responseType.Type);

                        if (canConvert)
                        {
                            _logger.LogDebug(
                                "{FileResultConverter} can handle file result conversion of {Type}. Modify the Api Description to reflect the runtime conversion.",
                                converter.GetType().Name, responseType.Type.Name);

                            responseType.Type = typeof(FileStreamResult);

                            break;
                        }

                        _logger.LogTrace("{FileResultConverter} can not handle {Type}. Trying the next IFileResultConverter.", converter.GetType().Name,
                            responseType.Type.Name);
                    }
                }
            }
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
        }
    }
}
