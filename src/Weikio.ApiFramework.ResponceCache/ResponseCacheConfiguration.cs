using System;
using System.Linq;

namespace Weikio.ApiFramework.ResponceCache
{
    public class ResponseCacheConfiguration
    {
        public TimeSpan MaxAge { get; }

        public string[] Vary { get; }

        public string[] VaryByQueryKeys { get; }

        public ResponseCacheConfiguration(TimeSpan maxAge, string[] vary, string[] varyByQueryKeys)
        {
            MaxAge = maxAge;
            Vary = vary;
            VaryByQueryKeys = varyByQueryKeys;
        }

        public override string ToString()
        {
            return $"MaxAge: {MaxAge}, Vary: {(Vary?.Any() == true ? string.Join(", ", Vary) : "")}, VaryByQueryKeys: {(VaryByQueryKeys?.Any() == true ? string.Join(", ", VaryByQueryKeys) : "")}";
        }
    }
}
