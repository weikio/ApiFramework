using System;

namespace Weikio.ApiFramework.Extensions.ResponceCache
{
    public class ResponseCacheConfiguration
    {
        public TimeSpan MaxAge { get; }

        public string[] Vary { get; }

        public ResponseCacheConfiguration(TimeSpan maxAge, string[] vary)
        {
            MaxAge = maxAge;
            Vary = vary;
        }
    }
}
