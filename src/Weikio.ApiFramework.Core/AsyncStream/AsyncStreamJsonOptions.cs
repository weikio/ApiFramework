using Microsoft.AspNetCore.Http.Features;

namespace Weikio.ApiFramework.Core.AsyncStream
{
    public class AsyncStreamJsonOptions
    {
        public bool IsEnabled { get; set; } = true;
        public int BufferSizeThreshold { get; set; } = 1024;
    }

}
