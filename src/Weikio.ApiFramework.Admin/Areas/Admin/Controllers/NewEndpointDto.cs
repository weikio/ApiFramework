using System.Collections.Generic;

namespace Weikio.ApiFramework.Admin.Areas.Admin.Controllers
{
    public class NewEndpointDto
    {
        public ApiDefinitionDto Api { get; set; }
        public string Route { get; set; }
        public Dictionary<string, object> JsonConfiguration { get; set; }
    }
}
