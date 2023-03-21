using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Controllers.Api
{
    [Feature("OrganiMedCore.Login.Api")]
    [ApiController, Route("api/useraccount")]
    [ValidateAntiForgeryToken]
    public class UserAccountApiController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly ILogger _logger;
        private readonly ISiteService _siteService;
        private readonly IEnumerable<ILoginFormEvent> _accountEvents;


        public IStringLocalizer T { get; set; }


        public UserAccountApiController(
            IUserService userService,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            ILogger<AccountController> logger,
            ISiteService siteService,
            IStringLocalizer<AccountController> stringLocalizer,
            IEnumerable<ILoginFormEvent> accountEvents)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
            _siteService = siteService;
            _accountEvents = accountEvents;

            T = stringLocalizer;
        }


        [HttpPost("login", Name = "UserApiLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if ((await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersMustValidateEmail)
            {
                // Require that the users have a confirmed email before they can log on.
                if (await _userManager.FindByNameAsync(model.UserName) is User user &&
                    !await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, T["You must have a confirmed email to log on."].Value);
                }
            }

            await _accountEvents.InvokeAsync(i => i.LoggingInAsync(model.UserName, (key, message) => ModelState.AddModelError(key, message)), _logger);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation(1, "User logged in.");
                await _accountEvents.InvokeAsync(a => a.LoggedInAsync(model.UserName), _logger);

                return Ok();
            }
            //if (result.RequiresTwoFactor)
            //{
            //    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            //}
            //if (result.IsLockedOut)
            //{
            //    _logger.LogWarning(2, "User account locked out.");
            //    return View("Lockout");
            //}
            else
            {
                ModelState.AddModelError(string.Empty, T["Invalid login attempt."]);
                await _accountEvents.InvokeAsync(a => a.LoggingInFailedAsync(model.UserName), _logger);

                return BadRequest(ModelState);
            }
        }

        [HttpPost("logoff", Name = "UserApiLogOff")]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");

            return Ok();
        }
    }
}
