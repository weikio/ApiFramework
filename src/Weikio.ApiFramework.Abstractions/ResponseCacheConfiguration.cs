using System;

namespace Weikio.ApiFramework.Abstractions
{
    public class ResponseCacheConfiguration
    {
        public string Path { get; }

        public TimeSpan MaxAge { get; }

        public string[] Vary { get; }

        public ResponseCacheConfiguration(string path, TimeSpan maxAge, string[] vary)
        {
            Path = path;
            MaxAge = maxAge;
            Vary = vary;
        }
    }
}
