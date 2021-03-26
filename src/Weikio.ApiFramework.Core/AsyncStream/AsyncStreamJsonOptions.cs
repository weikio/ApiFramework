using Microsoft.AspNetCore.Http.Features;

namespace Weikio.ApiFramework.Core.AsyncStream
{
    public class AsyncStreamJsonOptions
    {
        private bool _isEnabled = true;
        private int _bufferSizeThresholdInKb = 128;

        public bool IsConfigured { get; private set; } = false;

        /// <summary>
        /// Gets or sets if Api Framework should try to create Streaming APIs automatically from IAsyncEnumerable-methods
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                IsConfigured = true;
            }
        }

        /// <summary>
        /// Gets or sets the default buffer size threshold used by Streaming API. Defaults to 128KB
        /// </summary>
        public int BufferSizeThresholdInKB
        {
            get => _bufferSizeThresholdInKb;
            set
            {
                _bufferSizeThresholdInKb = value;
                IsConfigured = true;
            }
        }
    }
}
