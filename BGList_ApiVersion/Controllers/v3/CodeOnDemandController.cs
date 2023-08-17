using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BGList_ApiVersion.Controllers.v3
{
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("3.0")]
    public class CodeOnDemandController : ControllerBase
    {
        [HttpGet]
        [EnableCors("AnyOrigin_GetOnly")]
        [ResponseCache(NoStore = true)]
        public ContentResult Test2(int? minutesToAdd = null)
        {
            double minutes = minutesToAdd.HasValue ? minutesToAdd.Value : 0;
            return Content("<script>" +
                "window.alert('Your client supports JavaScript!" +
                "\\r\\n\\r\\n" +
                $"Server time (UTC): {DateTime.UtcNow.AddMinutes(minutes).ToString("o")}" +
                "\\r\\n" +
                "Client time (UTC): ' + new Date().toISOString());" +
                "</script>" +
                "<noscript>Your client does not support JavaScript</noscript>",
                "text/html"
            );
        }
    }
}
