using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Extensions;
using OrganiMedCore.Login.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Controllers
{
    public class OrganiMedCoreLoginController : Controller, IUpdateModel
    {
        private readonly IOrganiMedCoreLoginService _organiMedCoreLoginService;
        private readonly ISiteService _siteService;


        public OrganiMedCoreLoginController(
            IOrganiMedCoreLoginService organiMedCoreLoginService,
            ISiteService siteService)
        {
            _organiMedCoreLoginService = organiMedCoreLoginService;
            _siteService = siteService;
        }


        [HttpGet]
        [AllowAnonymous]
        [Route(NamedRoutes.OrganiMedCoreLoginPath)]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (!await _siteService.OrganiMedCoreLoginEnabledAsync())
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
        [Route(NamedRoutes.OrganiMedCoreLoginPath)]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!await _siteService.OrganiMedCoreLoginEnabledAsync())
            {
                return RedirectToAction("Login", "Account", new { area = "OrganiMedCore.Login" });
            }

            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var (success, foundSharedUser) = await _organiMedCoreLoginService.TrySharedLoginAsync(model, this);
                if (success)
                {
                    return RedirectToLocal(returnUrl);
                }

                if (foundSharedUser)
                {
                    return View(model);
                }
            }

            return View(model);
        }


        private IActionResult RedirectToLocal(string returnUrl)
            => Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "~/");
    }
}
