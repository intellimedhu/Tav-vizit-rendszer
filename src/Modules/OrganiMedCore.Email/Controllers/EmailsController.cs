using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Controllers
{
    [Admin]
    [Authorize]
    public class EmailsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;


        public EmailsController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }


        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            return View();
        }
    }
}
