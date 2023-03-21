using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenterTenant.Constants;
using OrganiMedCore.DiabetesCareCenterTenant.Extensions;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Exceptions;
using OrganiMedCore.Login.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterTenant.Controllers
{
    [Authorize]
    public class CenterProfileController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICenterProfileManagerService _centerProfileManagerService;
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;
        private readonly INotifier _notifier;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISharedUserService _sharedUserService;
        private readonly SignInManager<IUser> _signInManager;


        public IHtmlLocalizer T { get; set; }


        public CenterProfileController(
            IAuthorizationService authorizationService,
            ICenterProfileManagerService centerProfileManagerService,
            IDokiNetService dokiNetService,
            IHtmlLocalizer<CenterProfileController> htmlLocalizer,
            ILogger<CenterProfileController> logger,
            INotifier notifier,
            ISharedDataAccessorService sharedDataAccessorService,
            ISharedUserService sharedUserService,
            SignInManager<IUser> signInManager)
        {
            _authorizationService = authorizationService;
            _centerProfileManagerService = centerProfileManagerService;
            _dokiNetService = dokiNetService;
            _logger = logger;
            _notifier = notifier;
            _sharedDataAccessorService = sharedDataAccessorService;
            _sharedUserService = sharedUserService;
            _signInManager = signInManager;

            T = htmlLocalizer;
        }


        [Route("szakellatohely/attekintes")]
        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageCenterProfile))
            {
                return Unauthorized();
            }

            try
            {
                var contentItem = await _centerProfileManagerService.GetCenterProfileEditorForCurrentCenterAsync();
                if (!await _sharedUserService.AuthorizedToViewCenterProfile(contentItem))
                {
                    return Unauthorized();
                }

                var (currentState, detailsShape) = await _centerProfileManagerService.GetCenterProfileForCurrentCenterAsync(this);
                ViewData["CurrentState"] = currentState;
                ViewData["DetailsShape"] = detailsShape;

                return View();
            }
            catch (UserHasNoMemberRightIdException)
            {
                _notifier.Error(T["Sajnáljuk, de az Ön tagi adatai hiányosak a rendszerben, így a kért oldal nem tekinthető meg."]);

                return Redirect("~/");
            }
            catch (CenterProfileNotAssignedException)
            {
                return View("CenterProfileUnassigned");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> DccmLogin([FromQuery(Name = "h")]string nonce)
        {
            if (string.IsNullOrEmpty(nonce) || !Guid.TryParse(nonce, out var guid))
            {
                _notifier.Error(T["Érvénytelen hivatkozás."]);

                return RedirectToLogin();
            }

            var memberRightId = default(int?);
            try
            {
                using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
                {
                    var loginHandler = scope.ServiceProvider.GetRequiredService<IDccmCrossLoginHandler>();
                    memberRightId = await loginHandler.ValidateNonceAsync(guid);
                }

                if (!memberRightId.HasValue)
                {
                    _notifier.Error(T["Érvénytelen hivatkozás."]);

                    return RedirectToLogin();
                }

                var dokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(memberRightId.Value);
                if (dokiNetMember == null)
                {
                    return RedirectToLogin();
                }

                var contentItem = await _centerProfileManagerService.GetCenterProfileForCurrentCenterAsync();
                if (contentItem.As<CenterProfilePart>().MemberRightId != dokiNetMember.MemberRightId)
                {
                    _notifier.Error(T["A választot szakellátóhely nem Önhöz tartozik."]);

                    return RedirectToAction(nameof(Index));
                }

                User localUser = null;
                using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                {
                    localUser = await _sharedUserService.CreateOrUpdateSharedUserUsingDokiNetMemberAsync(
                        scope,
                        dokiNetMember,
                        new List<string>()
                        {
                            DiabetesCareCenterRoleNames.DiabetesCareCenterLeader
                        },
                        this) as User;
                }

                if (User.Identity.IsAuthenticated)
                {
                    await _signInManager.SignOutAsync();
                }

                await _signInManager.SignInAsync(localUser, isPersistent: false);
                if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
                {
                    _notifier.Warning(T["A szakellátóhely akkreditációs megújítása folyamatban van, ezért az adatlap jelenleg nem szerkeszthető."]);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (CenterProfileNotAssignedException)
            {
                return View("CenterProfileUnassigned");
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (DokiNetMemberRegistrationException ex)
            {
                _notifier.Error(T[ex.Message]);

                return LocalRedirect("~/");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", memberRightId);

                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                return LocalRedirect("~/");
            }
        }

        [Route("szakellatohely/szerkesztes")]
        public async Task<IActionResult> Edit()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageCenterProfile))
            {
                return Unauthorized();
            }

            try
            {
                var contentItem = await _centerProfileManagerService.GetCenterProfileEditorForCurrentCenterAsync(true);
                if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
                {
                    _notifier.Warning(T["A szakellátóhely akkreditációs megújítása folyamatban van, ezért az adatlap jelenleg nem szerkeszthető."]);

                    return RedirectToAction(nameof(Index));
                }

                if (!await _sharedUserService.AuthorizedToViewCenterProfile(contentItem))
                {
                    return Unauthorized();
                }

                ViewData["CurrentState"] = contentItem.As<CenterProfileReviewStatesPart>().GetCurrentReviewState();

                using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
                {
                    var siteSettings = await scope.ServiceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync();
                    ViewData["GoogleMapsApiKey"] = siteSettings.As<CenterManagerSettings>().GoogleMapsApiKey;
                }

                return View();
            }
            catch (CenterProfileNotAssignedException)
            {
                return View("CenterProfileUnassigned");
            }
        }


        private IActionResult RedirectToLogin()
            => LocalRedirect($"~/{NamedRoutes.DokiNetLoginPath}?returnUrl=" + Url.Action(nameof(Index), "CenterProfile"));

        private IActionResult RedirectUnauthorized()
        {
            _notifier.Error(T["Önnen nincs joga megtekinteni a kért oldalt"]);

            return LocalRedirect("~/");
        }
    }
}
