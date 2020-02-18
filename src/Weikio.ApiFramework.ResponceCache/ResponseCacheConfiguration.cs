using System;
using System.Linq;

namespace Weikio.ApiFramework.ResponceCache
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

        public override string ToString()
        {
            return $"MaxAge: {MaxAge}, Vary: {(Vary?.Any() == true ? string.Join(", ", Vary) : "")}";
        }
    }
}
