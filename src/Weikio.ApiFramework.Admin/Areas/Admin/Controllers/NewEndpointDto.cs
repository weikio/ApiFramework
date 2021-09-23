using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    public class NewEndpointDto
    {
        public ApiDefinitionDto Api { get; set; }
        public string Route { get; set; }
        [JsonConverter(typeof(DictionaryConverter))]
        public Dictionary<string, object> JsonConfiguration { get; set; }
    }
}
