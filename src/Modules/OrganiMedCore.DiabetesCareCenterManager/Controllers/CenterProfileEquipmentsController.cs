using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrganiMedCore.DiabetesCareCenter.Core;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Admin]
    [Authorize]
    public class CenterProfileEquipmentsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;


        public CenterProfileEquipmentsController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }


        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterProfileAccreditationConditionsSettings))
            {
                return Unauthorized();
            }

            return View();
        }
    }
}
