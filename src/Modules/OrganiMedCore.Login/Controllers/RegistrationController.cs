using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using OrganiMedCore.Login.Services;
using OrganiMedCore.Login.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Controllers
{
    [Feature("OrganiMedCore.Login.Registration")]
    public class RegistrationController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISharedLoginService _sharedLoginService;
        private readonly SignInManager<IUser> _signInManager;


        public IStringLocalizer T { get; set; }


        public RegistrationController(
            ISiteService siteService,
            ISharedDataAccessorService sharedDataAccessorService,
            ISharedLoginService sharedLoginService,
            SignInManager<IUser> signInManager,
            IStringLocalizer<RegistrationController> stringLocalizer)
        {
            _siteService = siteService;
            _sharedDataAccessorService = sharedDataAccessorService;
            _sharedLoginService = sharedLoginService;
            _signInManager = signInManager;

            T = stringLocalizer;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersCanRegister)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(EVisitRegisterViewModel model, string returnUrl = null)
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();

            if (!settings.UsersCanRegister)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    var user = await _sharedLoginService.SharedRegisterAsync(
                        scope,
                        model.Email,
                        !settings.UsersMustValidateEmail,
                        model.Password,
                        new List<string>(),
                        null,
                        (key, message) => ModelState.AddModelError(key, message));
                    if (user != null)
                    {
                        if (settings.UsersMustValidateEmail)
                        {
                            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                            // Send an email with this link
                            await SendEmailConfirmationTokenAsync(scope, user as User);
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, false);
                        }

                        return RedirectToLocal(returnUrl);
                    }

                    ModelState.AddModelError(string.Empty, "OrganiMed felhasználó regisztrációja sikertelen.");
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(Register), "Registration");
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var managerUserManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

                var user = await managerUserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }
                var result = await managerUserManager.ConfirmEmailAsync(user, code);

                return View(result.Succeeded ? "ConfirmEmail" : "Error");
            }
        }


        private async Task<string> SendEmailConfirmationTokenAsync(IServiceScope scope, User user)
        {
            var managerUserManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            var code = await managerUserManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Registration", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
            await _sharedLoginService.SendEmailAsync(scope, user, ControllerContext, Url, HttpContext, ViewData, TempData, user.Email, T["Confirm your account"], new ConfirmEmailViewModel() { User = user, ConfirmEmailUrl = callbackUrl }, "TemplateUserConfirmEmail");

            return callbackUrl;
        }

        private IActionResult RedirectToLocal(string returnUrl)
            => Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "~/");
    }
}
