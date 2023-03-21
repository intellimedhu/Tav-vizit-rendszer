using IntelliMed.Core.Constants;
using Microsoft.AspNetCore.Mvc;

namespace IntelliMed.Core.Controllers
{
    [Route(KeepAliveConstants.KeepAliveRelativePath)]
    [ApiController]
    public class KeepAliveController : Controller
    {
        [HttpHead("")]
        public IActionResult Head() => Ok();
    }
}
