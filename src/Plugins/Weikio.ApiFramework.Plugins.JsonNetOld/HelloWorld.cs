using System;
using Newtonsoft.Json;

namespace Weikio.ApiFramework.Plugins.JsonNetOld
{
    public class OldJsonApi
    {
        public Version ReturnVersion()
        {
            var result = typeof(JsonConvert).Assembly.GetName().Version;

            return result;
        }
        
        public DataStructure GetData()
        {
            return new DataStructure();
        }
    }

    public class DataStructure
    {
        public string Name { get; set; } = "Hello From old schema";
        public int Age { get; set; } = 50;
    }
}
