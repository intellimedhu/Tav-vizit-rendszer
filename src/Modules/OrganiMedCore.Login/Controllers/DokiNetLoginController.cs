using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Extensions;
using OrganiMedCore.Login.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Controllers
{
    public class DokiNetLoginController : Controller, IUpdateModel
    {
        private readonly IDokiNetLoginService _dokiNetLoginService;
        private readonly ISiteService _siteService;


        public IHtmlLocalizer T { get; set; }


        public DokiNetLoginController(
            IDokiNetLoginService dokiNetLoginService,
            IHtmlLocalizer<DokiNetLoginController> htmlLocalizer,
            ISiteService siteService)
        {
            _dokiNetLoginService = dokiNetLoginService;
            _siteService = siteService;

            T = htmlLocalizer;
        }


        [HttpGet]
        [AllowAnonymous]
        [Route(NamedRoutes.DokiNetLoginPath)]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (!await _siteService.DokiNetLoginEnabledAsync())
            {
                return RedirectToAction("Login", "Account", new { area = "OrganiMedCore.Login" });
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route(NamedRoutes.DokiNetLoginPath)]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!await _siteService.DokiNetLoginEnabledAsync())
            {
                return RedirectToAction("Login", "Account", new { area = "OrganiMedCore.Login" });
            }

            if (ModelState.IsValid && await _dokiNetLoginService.TryDokiNetLoginAsync(model, this))
            {
                return RedirectToLocal(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }


        private IActionResult RedirectToLocal(string returnUrl)
            => Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "~/");
    }
}
