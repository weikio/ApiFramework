using System;
using Newtonsoft.Json;

namespace Weikio.ApiFramework.Plugins.JsonNetOld
{
    public class OldJsonFunction
    {
        public Version ReturnVersion()
        {
            var result = typeof(JsonConvert).Assembly.GetName().Version;

            return result;
        }
    }
}
