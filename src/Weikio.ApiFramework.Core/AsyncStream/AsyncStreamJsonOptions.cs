using Microsoft.AspNetCore.Http.Features;

namespace Weikio.ApiFramework.Core.AsyncStream
{
    public class AsyncStreamJsonOptions
    {
        /// <summary>
        /// Gets or sets if Api Framework should try to create Streaming APIs automatically from IAsyncEnumerable-methods
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the default buffer size threshold used by Streaming API. Defaults to 128KB
        /// </summary>
        public int BufferSizeThreshold { get; set; } = 128 * 1000;
    }

}
