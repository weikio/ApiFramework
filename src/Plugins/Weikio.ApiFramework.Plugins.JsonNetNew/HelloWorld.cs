using System;
using Newtonsoft.Json;

namespace Weikio.ApiFramework.Plugins.JsonNetNew
{
    public class NewJsonFunction
    {
        public Version ReturnVersion()
        {
            var result = typeof(JsonConvert).Assembly.GetName().Version;

            return result;
        }
    }
}
