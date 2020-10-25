using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.AspNetCore.NSwag
{
    public class OpenApiDocumentExtensions
    {
        public Dictionary<string, OpenApiPathItem> AdditionalOperationPaths { get; set; }
        public List<KeyValuePair<string, JsonSchema>> AdditionalSchemas { get; set; }

        public OpenApiDocumentExtensions(Dictionary<string, OpenApiPathItem> additionalOperationPaths, List<KeyValuePair<string, JsonSchema>> additionalSchemas)
        {
            AdditionalOperationPaths = additionalOperationPaths;
            AdditionalSchemas = additionalSchemas;
        }
    }
    
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
                if (endpoint.ExtendedMetadata?.Any() != true)
                {
                    continue;
                }

                foreach (var extendedMetadata in endpoint.ExtendedMetadata)
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
