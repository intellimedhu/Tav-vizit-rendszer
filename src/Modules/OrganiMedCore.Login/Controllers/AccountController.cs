using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Services;
using OrganiMedCore.Login.Settings;
using OrganiMedCore.Login.Settings.Enums;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Controllers
{
    /// <summary>
    /// This controller is basically a copy of <see cref="OrchardCore.Users.Controllers.AccountController"/>.
    /// Keep this and it's views in sync with the original controller when updating OrchardCore's nuget packages.
    /// The Login, Register, ChangePassword (POSTs) actions were modified mostly.
    /// </summary>
    [Authorize]
    public class AccountController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly ISharedLoginService _sharedLoginService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public AccountController(
            ISiteService siteService,
            ISharedLoginService sharedLoginService,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _siteService = siteService;
            _sharedLoginService = sharedLoginService;
            _sharedDataAccessorService = sharedDataAccessorService;
        }


        [HttpGet]
        [AllowAnonymous]
        [Route(NamedRoutes.OrganiMedCoreLoginRouter)]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<OmcLoginSettings>();

            if (settings.DefaultLoginMethod == OmcLoginMethods.DokiNet && settings.UseDokiNetLogin)
            {
                return RedirectToAction("Login", "DokiNetLogin", new
                {
                    area = "OrganiMedCore.Login",
                    returnUrl
                });
            }

            if (settings.DefaultLoginMethod == OmcLoginMethods.OrganiMedCore && settings.UseOrganiMedCoreLogin)
            {
                return RedirectToAction("Login", "OrganiMedCoreLogin", new
                {
                    area = "OrganiMedCore.Login",
                    returnUrl
                });
            }

            return RedirectToAction("Login", "Account", new
            {
                area = "OrchardCore.Users",
                returnUrl
            });
        }

        [HttpGet]
        [Route(NamedRoutes.OrganiMedCoreChangePasswordPath)]
        public IActionResult ChangePassword()
            => View();

        [HttpPost]
        [Route(NamedRoutes.OrganiMedCoreChangePasswordPath)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    if (await _sharedLoginService.ChangeSharedUsersPasswordAsync(scope, User, model.CurrentPassword, model.Password, (key, message) => ModelState.AddModelError(key, message)))
                    {
                        return RedirectToLocal(Url.Action("ChangePasswordConfirmation"));
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
            => View();

        [AllowAnonymous]
        [Route("hozzaferes-megtagadva")]
        public IActionResult AccessDenied()
            => View();

        private IActionResult RedirectToLocal(string returnUrl)
            => Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "~/");
    }
}
