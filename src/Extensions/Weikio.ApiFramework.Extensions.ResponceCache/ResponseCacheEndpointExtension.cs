using Weikio.ApiFramework.Abstractions;

namespace Weikio.ApiFramework.Extensions.ResponceCache
{
    public class ResponseCacheEndpointExtension : IEndpointExtension
    {
        public ResponseCacheEndpointExtension(ResponseCacheConfiguration data)
        {
            Data = data;
        }

        public string Key { get; } = "RESPONSE_CACHE";
        public object Data { get; set; }
    }
}
