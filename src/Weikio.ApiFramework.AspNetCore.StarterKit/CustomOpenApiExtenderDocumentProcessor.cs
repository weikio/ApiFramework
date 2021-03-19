using System;
using System.Linq;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.ApiFramework.AspNetCore.StarterKit
{
    public class CustomOpenApiExtenderDocumentProcessor : IDocumentProcessor
    {
        private readonly IEndpointManager _endpointManager;

        public CustomOpenApiExtenderDocumentProcessor(IEndpointManager endpointManager)
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

                var found = false;

                if (context.Settings is AspNetCoreOpenApiDocumentGeneratorSettings settings)
                {
                    if (endpoint != null && !string.IsNullOrWhiteSpace(endpoint.GroupName) && settings.ApiGroupNames?.Any() == true)
                    {
                        foreach (var apiGroupName in settings.ApiGroupNames)
                        {
                            if (string.Equals(apiGroupName, endpoint.GroupName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                found = true;

                                break;
                            }

                            if (string.Equals(apiGroupName, "api_framework_endpoint", StringComparison.InvariantCultureIgnoreCase))
                            {
                                found = true;

                                break;
                            }
                        }

                        if (!found)
                        {
                            continue;
                        }
                    }
                    
                    if (endpoint != null && settings.ApiGroupNames?.Any() == true && !found)
                    {
                        foreach (var apiGroupName in settings.ApiGroupNames)
                        {
                            if (string.Equals(apiGroupName, "api_framework_endpoint", StringComparison.InvariantCultureIgnoreCase))
                            {
                                found = true;

                                break;
                            }
                        }

                        if (!found)
                        {
                            continue;
                        }
                    }

                }

                foreach (var extendedMetadata in endpoint.Metadata)
                {
                    if (extendedMetadata is NSwag.OpenApiDocumentExtensions openApiContent)
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
                                // var schemaWithPrefix = new KeyValuePair<string, JsonSchema>(endpoint.Route.Trim('/') + "_" + schema.Key, schema.Value);
                                context.Document.Components.Schemas.Add(schema);
                            }
                        }
                    }
                }
            }
        }
    }
}
