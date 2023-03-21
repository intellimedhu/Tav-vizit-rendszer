using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Controllers
{
    [Feature("OrganiMedCore.Login.ResetPassword")]
    public class ResetPasswordController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISharedLoginService _sharedLoginService;


        public IStringLocalizer T { get; set; }


        public ResetPasswordController(
            ISiteService siteService,
            IStringLocalizer<ResetPasswordController> stringLocalizer,
            ISharedDataAccessorService sharedDataAccessorService,
            ISharedLoginService sharedLoginService)
        {
            _siteService = siteService;
            _sharedDataAccessorService = sharedDataAccessorService;
            _sharedLoginService = sharedLoginService;

            T = stringLocalizer;
        }


        [HttpGet]
        [AllowAnonymous]
        [Route(NamedRoutes.OrganiMedCoreForgotPasswordPath)]
        public async Task<IActionResult> ForgotPassword()
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(NamedRoutes.OrganiMedCoreForgotPasswordPath)]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    await _sharedLoginService.SharedForgotPasswordAsync(scope, model.UserIdentifier, ControllerContext, Url, HttpContext, ViewData, TempData);

                    return RedirectToLocal(Url.Action("ForgotPasswordConfirmation", "Password"));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string code = null)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }
            if (code == null)
            {
                //"A code must be supplied for password reset.";
            }
            return View(new ResetPasswordViewModel { ResetToken = code });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().AllowResetPassword)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    if (await _sharedLoginService.SharedResetPasswordAsync(scope, model.Email, model.ResetToken, model.NewPassword, (key, message) => ModelState.AddModelError(key, message)))
                    {
                        return RedirectToLocal(Url.Action("ResetPasswordConfirmation", "ResetPassword"));
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        private IActionResult RedirectToLocal(string returnUrl)
            => Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "~/");
    }
}