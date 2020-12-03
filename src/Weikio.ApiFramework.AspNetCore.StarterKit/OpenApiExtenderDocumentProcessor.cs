using System.Linq;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.AspNetCore.StarterKit
{
    public class OpenApiExtenderDocumentProcessor : IDocumentProcessor
    {
        private readonly EndpointManager _endpointManager;

        public OpenApiExtenderDocumentProcessor(EndpointManager endpointManager)
        {
            _endpointManager = endpointManager;
        }

        public void Process(DocumentProcessorContext context)
        {
            var endpoints = _endpointManager.Endpoints;

            foreach (var endpoint in endpoints)
            {
                if (endpoint.Metadata?.Any() != true)
                {
                    continue;
                }

                foreach (var extendedMetadata in endpoint.Metadata)
                {
                    if (extendedMetadata is OpenApiDocumentExtensions openApiContent)
                    {
                        if (openApiContent.AdditionalOperationPaths?.Any() == true)
                        {
                            foreach (var path in openApiContent.AdditionalOperationPaths)
                            {
                                context.Document.Paths.Add(path.Key, path.Value);
                            }
                        }
                        
                        if (openApiContent.AdditionalSchemas?.Any() == true)
                        {
                            foreach (var schema in openApiContent.AdditionalSchemas)
                            {
                                context.Document.Components.Schemas.Add(schema);
                            }
                        }
                    }
                }
            }
        }
    }
}
