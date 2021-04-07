using System;
using Newtonsoft.Json;

namespace Weikio.ApiFramework.Plugins.JsonNetNew
{
    public class NewJsonApi
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
        public string Name { get; set; } = "Hello from new schema";
    }
}
