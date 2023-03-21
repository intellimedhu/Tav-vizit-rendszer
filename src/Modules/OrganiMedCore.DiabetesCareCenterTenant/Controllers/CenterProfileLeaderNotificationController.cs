using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.DiabetesCareCenterTenant.Constants;
using OrganiMedCore.DiabetesCareCenterTenant.Settings;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterTenant.Controllers
{
    [Admin]
    public class CenterProfileLeaderNotificationController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICenterProfileManagerService _centerProfileManagerService;
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;
        private readonly INotifier _notifier;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISiteService _siteService;


        public IHtmlLocalizer T { get; set; }


        public CenterProfileLeaderNotificationController(
            IAuthorizationService authorizationService,
            ICenterProfileManagerService centerProfileManagerService,
            IDokiNetService dokiNetService,
            IHtmlLocalizer<CenterProfileLeaderNotificationController> htmlLocalizer,
            ILogger<CenterProfileLeaderNotificationController> logger,
            INotifier notifier,
            ISharedDataAccessorService sharedDataAccessorService,
            ISiteService siteService)
        {
            _authorizationService = authorizationService;
            _centerProfileManagerService = centerProfileManagerService;
            _dokiNetService = dokiNetService;
            _logger = logger;
            _notifier = notifier;
            _sharedDataAccessorService = sharedDataAccessorService;
            _siteService = siteService;

            T = htmlLocalizer;
        }


        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            var contentItem = await _centerProfileManagerService.GetCenterProfileForCurrentCenterAsync();
            if (contentItem == null)
            {
                _notifier.Error(T["A szakellátóhely nem lett kiválasztva!"]);

                return RedirectToSettings();
            }

            var memberRightId = 0;
            try
            {
                var part = contentItem.As<CenterProfilePart>();
                memberRightId = part.MemberRightId;
                var leader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(memberRightId);

                using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
                {
                    var nonce = new Nonce()
                    {
                        RedirectUrl = "~/" + CenterProfileNamedRoutes.CenterProfile_MyCenterProfiles,
                        Type = NonceType.MemberRightId,
                        TypeId = memberRightId
                    };
                    await scope.ServiceProvider.GetRequiredService<INonceService>().CreateAsync(nonce);

                    await scope.ServiceProvider
                        .GetRequiredService<IEmailNotificationDataService>()
                        .QueueAsync(new EmailNotification()
                        {
                            TemplateId = EmailTemplateIds.CenterProfileAssignedOnTenant,
                            To = new HashSet<string>(new[] { leader.Emails.First() }),
                            Data = new CenterProfileAssignedOnTenantViewModel()
                            {
                                CenterName = part.CenterName,
                                LeaderName = leader.FullName,
                                Nonce = nonce.Value
                            }
                        });
                }

                var siteSettings = await _siteService.GetSiteSettingsAsync();
                if (!siteSettings.As<CenterSettings>().LeaderNotified)
                {
                    siteSettings.Alter<CenterSettings>(nameof(CenterSettings), settings => settings.LeaderNotified = true);
                    await _siteService.UpdateSiteSettingsAsync(siteSettings);
                }

                _notifier.Success(T["Az email-es értesítés sikeresen elküldve: {0}.", leader.Emails.First()]);
            }
            catch (HttpRequestException ex)
            {
                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", memberRightId);
            }

            return RedirectToSettings();
        }


        private RedirectToActionResult RedirectToSettings()
            => RedirectToAction("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GroupIds.CenterSettings });
    }
}
