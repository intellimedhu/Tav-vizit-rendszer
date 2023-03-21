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
    [ApiController, Route("api/dokinetuser")]
    [ValidateAntiForgeryToken]
    public class DokiNetLoginApiController : Controller, IUpdateModel
    {
        private readonly IDokiNetLoginService _dokiNetLoginService;
        private readonly ISiteService _siteService;


        public DokiNetLoginApiController(
            IDokiNetLoginService dokiNetLoginService,
            ISiteService siteService)
        {
            _dokiNetLoginService = dokiNetLoginService;
            _siteService = siteService;
        }


        [HttpPost("login", Name = "DokiNetUserLogin")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!await _siteService.DokiNetLoginEnabledAsync())
            {
                return StatusCode(405);
            }

            if (ModelState.IsValid && await _dokiNetLoginService.TryDokiNetLoginAsync(model, this))
            {
                return Ok();
            }

            return BadRequest(ModelState);
        }
    }
}
