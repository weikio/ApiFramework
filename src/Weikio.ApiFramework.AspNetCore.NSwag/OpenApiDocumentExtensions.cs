using System.Collections.Generic;
using NJsonSchema;
using NSwag;

namespace Weikio.ApiFramework.AspNetCore.NSwag
{
    public class OpenApiDocumentExtensions
    {
        public Dictionary<string, OpenApiPathItem> AdditionalOperationPaths { get; }
        public List<KeyValuePair<string, JsonSchema>> AdditionalSchemas { get; }

        public OpenApiDocumentExtensions(Dictionary<string, OpenApiPathItem> additionalOperationPaths, List<KeyValuePair<string, JsonSchema>> additionalSchemas)
        {
            AdditionalOperationPaths = additionalOperationPaths;
            AdditionalSchemas = additionalSchemas;
        }
    }
}
