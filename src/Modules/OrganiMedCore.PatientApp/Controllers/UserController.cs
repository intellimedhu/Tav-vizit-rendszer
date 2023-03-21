using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrganiMedCore.PatientApp.Controllers
{
    [ApiController, Route("api/[controller]/[action]")]
    public class UserController : Controller
    {
        [Authorize]
        public IActionResult Name() 
            => Ok(User.Identity.Name);
    }
}
