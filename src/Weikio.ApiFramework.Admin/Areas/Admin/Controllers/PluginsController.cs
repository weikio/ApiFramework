//using System.Collections.Generic;
//using Microsoft.AspNetCore.Mvc;
//using System.Linq;
//
//namespace FunctionFramework.Admin.Controllers
//{
//    [ApiController]
//    [Route("/admin/api/plugins")]
//    [ApiExplorerSettings(GroupName = "admin")]
//    public class PluginsController : ControllerBase
//    {
//        private readonly FunctionCatalog _functionCatalog;
//
//        public PluginsController(FunctionCatalog functionCatalog)
//        {
//            _functionCatalog = functionCatalog;
//        }
//
//        [HttpGet]
//        public ActionResult<IEnumerable<PluginDto>> GetAvailablePlugins()
//        {
//            return Ok(_functionCatalog.Select(x => new PluginDto(x)));
//        }
//    }
//}


