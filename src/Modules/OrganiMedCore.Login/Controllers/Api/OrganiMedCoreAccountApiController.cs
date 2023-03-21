using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Login.Extensions;
using OrganiMedCore.Login.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Controllers.Api
{
    [Feature("OrganiMedCore.Login.Api")]
    [ApiController]
    [Route("api/omc-account", Name = "OrganiMedCoreAccountApi")]
    [ValidateAntiForgeryToken]
    public class OrganiMedCoreAccountApiController : Controller, IUpdateModel
    {
        private readonly IOrganiMedCoreLoginService _organiMedCoreLoginService;
        private readonly ISiteService _siteService;


        public OrganiMedCoreAccountApiController(IOrganiMedCoreLoginService organiMedCoreLoginService, ISiteService siteService)
        {
            _organiMedCoreLoginService = organiMedCoreLoginService;
            _siteService = siteService;
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!await _siteService.OrganiMedCoreLoginEnabledAsync())
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var (success, _) = await _organiMedCoreLoginService.TrySharedLoginAsync(model, this);
                if (success)
                {
                    return Ok();
                }
            }

            return BadRequest(ModelState);
        }
    }
}
