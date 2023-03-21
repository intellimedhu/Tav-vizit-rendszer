using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Services
{
    // TODO: add unit tests
    public class OrganiMedCoreLoginService : IOrganiMedCoreLoginService
    {
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISharedLoginService _sharedLoginService;
        private readonly ISiteService _siteService;
        private readonly SignInManager<IUser> _signInManager;


        public IStringLocalizer T { get; set; }


        public OrganiMedCoreLoginService(
            ISharedDataAccessorService sharedDataAccessorService,
            ISharedLoginService sharedLoginService,
            ISiteService siteService,
            IStringLocalizer<OrganiMedCoreLoginService> stringLocalizer,
            SignInManager<IUser> signInManager)
        {
            _sharedDataAccessorService = sharedDataAccessorService;
            _sharedLoginService = sharedLoginService;
            _siteService = siteService;
            _signInManager = signInManager;

            T = stringLocalizer;
        }


        public async Task<(bool success, bool foundUser)> TrySharedLoginAsync(LoginViewModel model, IUpdateModel updater)
        {
            IUser localUser = null;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                if ((await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersMustValidateEmail)
                {
                    var managersUserManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                    // Require that the users have a confirmed email before they can log on.
                    if (await managersUserManager.FindByNameAsync(model.UserName) is User existingUser)
                    {
                        if (!await managersUserManager.IsEmailConfirmedAsync(existingUser))
                        {
                            updater.ModelState.AddModelError(string.Empty, T["Nincs megerősítve a regisztráció."]);

                            return (false, true);
                        }
                    }
                }

                var sharedUser = await _sharedLoginService.FindSharedUserByUserNameAsync(scope, model.UserName);
                if (sharedUser == null)
                {
                    updater.ModelState.AddModelError(string.Empty, T["Sikertelen bejelentkezés."].Value);

                    return (false, false);
                }

                if (!_sharedLoginService.AuthorizedToSignInOnTenant(sharedUser))
                {
                    updater.ModelState.AddModelError(string.Empty, T["A bejelentkezés nem engedélyezett."].Value);

                    return (false, false);
                }

                localUser = await _sharedLoginService.CheckPasswordSignInAsync(scope, sharedUser, model.Password);
                if (localUser == null)
                {
                    updater.ModelState.AddModelError(string.Empty, T["Sikertelen bejelentkezés."].Value);

                    return (false, true);
                }
            }

            await _signInManager.SignInAsync(localUser, false);

            return (true, true);
        }
    }
}
