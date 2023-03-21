using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using IntelliMed.DokiNetIntegration.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.DiabetesCareCenterManager.Extensions;
using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Login.Exceptions;
using OrganiMedCore.Login.Services;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Authorize]
    public class CenterProfileController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IBetterUserService _betterUserService;
        private readonly ICenterProfileService _centerProfileService;
        private readonly ICenterProfileCommonService _centerProfileCommonService;
        private readonly ICenterProfileReviewService _centerProfileReviewService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IDccmCrossLoginHandler _dccmCrossLoginHandler;
        private readonly IDiabetesUserProfileService _diabetesUserProfileService;
        private readonly IDokiNetService _dokiNetService;
        private readonly IEmailNotificationDataService _emailNotificationDataService;
        private readonly IEmailSettingsService _emailSettingsService;
        private readonly ILogger _logger;
        private readonly INonceService _nonceService;
        private readonly INotifier _notifier;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISharedUserService _sharedUserService;
        private readonly ITerritoryService _territoryService;
        private readonly UserManager<IUser> _userManager;


        public IHtmlLocalizer T { get; set; }


        public CenterProfileController(
            IAuthorizationService authorizationService,
            IBetterUserService betterUserService,
            ICenterProfileService centerProfileService,
            ICenterProfileCommonService centerProfileCommonService,
            ICenterProfileReviewService centerProfileReviewService,
            IContentItemDisplayManager contentItemDisplayManager,
            IDccmCrossLoginHandler dccmCrossLoginHandler,
            IDiabetesUserProfileService diabetesUserProfileService,
            IDokiNetService dokiNetService,
            IEmailNotificationDataService emailNotificationDataService,
            IEmailSettingsService emailSettingsService,
            IHtmlLocalizer<CenterProfileController> htmlLocalizer,
            ILogger<CenterProfileController> logger,
            INonceService nonceService,
            INotifier notifier,
            ISharedDataAccessorService sharedDataAccessorService,
            ISharedUserService sharedUserService,
            ITerritoryService territoryService,
            UserManager<IUser> userManager)
        {
            _authorizationService = authorizationService;
            _betterUserService = betterUserService;
            _centerProfileService = centerProfileService;
            _centerProfileCommonService = centerProfileCommonService;
            _centerProfileReviewService = centerProfileReviewService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _dccmCrossLoginHandler = dccmCrossLoginHandler;
            _diabetesUserProfileService = diabetesUserProfileService;
            _dokiNetService = dokiNetService;
            _emailNotificationDataService = emailNotificationDataService;
            _emailSettingsService = emailSettingsService;
            _logger = logger;
            _nonceService = nonceService;
            _notifier = notifier;
            _sharedDataAccessorService = sharedDataAccessorService;
            _sharedUserService = sharedUserService;
            _territoryService = territoryService;
            _userManager = userManager;

            T = htmlLocalizer;
        }


        [Route("szakellatohely/lista")]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ViewListOfCenterProfiles))
                {
                    return Unauthorized();
                }

                var contentItems = Enumerable.Empty<ContentItem>();
                if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ViewAllCenterProfiles))
                {
                    contentItems = await _centerProfileService.GetCenterProfilesAsync();
                }
                else if (User.IsInRole(CenterPosts.TerritorialRapporteur))
                {
                    contentItems = await _centerProfileService.GetPermittedCenterProfilesAsync();
                }
                else
                {
                    return Unauthorized();
                }

                return await CenterProfilePartsResultAsync(contentItems, nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMembersByIds");

                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                return this.RedirectToHome();
            }
        }

        [Route("szakellatohely/kepesites")]
        public async Task<IActionResult> MyCenterProfilesPrerequisites()
        {
            var reditectTo = nameof(MyCenterProfiles);

            return await MyCenterProfilesCommon((hasMissingQualifications, dokiNetMember) =>
            {
                var result = !hasMissingQualifications
                    ? RedirectToAction(reditectTo)
                    : (IActionResult)View();

                return Task.FromResult(result);
            });
        }

        [Route(CenterProfileNamedRoutes.CenterProfile_MyCenterProfiles)]
        public async Task<IActionResult> MyCenterProfiles()
            => await MyCenterProfilesCommon(async (hasMissingQualifications, dokiNetMember) =>
            {
                if (hasMissingQualifications)
                {
                    return RedirectToAction(nameof(MyCenterProfilesPrerequisites));
                }

                var contentItems = await _centerProfileService.GetCenterProfilesToLeaderAsync(dokiNetMember.MemberRightId);

                var viewModels = contentItems.Select(contentItem =>
                    CenterProfileComplexViewModel.CreateViewModel(contentItem, basicData: true, renewal: true, member: dokiNetMember));

                return View(viewModels);
            });

        public async Task<IActionResult> DeleteOwnCenterProfile(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfile))
            {
                return RedirectWhenUserIsNotDoctor();
            }

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest();
                }

                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
                if (dokiNetMember == null || dokiNetMember.MemberRightId == 0)
                {
                    return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
                }

                await _centerProfileService.DeleteOwnCenterProfileAsync(id, dokiNetMember.MemberRightId);

                _notifier.Success(T["A szakellátóhely törlésre került."]);

                return RedirectToAction(nameof(MyCenterProfiles));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (CenterProfileAreadyCreatedException)
            {
                _notifier.Warning(T["A szakellátóhely létrehozási kérelem már el lett küldve, így a törlés nem teljesíthető."]);

                return RedirectToAction(nameof(MyCenterProfiles));
            }
            catch (UserHasNoMemberRightIdException)
            {
                return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
            }
        }

        [Route("szakellatohely/letrehozas/{id?}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfile))
            {
                return RedirectWhenUserIsNotDoctor();
            }

            try
            {
                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
                if (dokiNetMember == null || dokiNetMember.MemberRightId == 0)
                {
                    return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
                }

                var isNew = string.IsNullOrEmpty(id);
                if (!isNew && await _centerProfileService.GetCenterProfileToLeaderAsync(dokiNetMember.MemberRightId, id) == null)
                {
                    return NotFound();
                }

                ViewData["GoogleMapsApiKey"] = (await _centerProfileCommonService.GetCenterManagerSettingsAsync()).GoogleMapsApiKey;
                ViewData["Id"] = id;

                return View();
            }
            catch (UserHasNoMemberRightIdException)
            {
                return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
            }
        }

        [Route("szakellatohely/szerkesztes/{id}")]
        public async Task<IActionResult> Forward(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfile))
            {
                return RedirectWhenUserIsNotDoctor();
            }

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            try
            {
                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
                if (dokiNetMember == null || dokiNetMember.MemberRightId == 0)
                {
                    return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
                }

                var centerProfile = await _centerProfileService.GetCenterProfileToLeaderAsync(dokiNetMember.MemberRightId, id);
                if (centerProfile == null)
                {
                    return NotFound();
                }

                var part = centerProfile.As<CenterProfileManagerExtensionsPart>();
                if (string.IsNullOrEmpty(part.AssignedTenantName))
                {
                    _notifier.Warning(T["A szakellátóhely létrehozási kérelem, még függőben van."]);

                    return RedirectToAction(nameof(MyCenterProfiles));
                }

                if (part.Submitted())
                {
                    _notifier.Warning(T["A szakellátóhely akkreditációs megújítása folyamatban van, ezért az adatlap jelenleg nem szerkeszthető."]);

                    return RedirectToAction(nameof(MyCenterProfiles));
                }


                string baseUrl = null;
                string requestUrlPrefix = null;
                using (var scope = await _sharedDataAccessorService.GetTenantServiceScopeAsync(part.AssignedTenantName))
                {
                    var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
                    baseUrl = shellSettings.RequestUrlHost;
                    requestUrlPrefix = shellSettings.RequestUrlPrefix;
                }

                string basePath = null;
                if (string.IsNullOrEmpty(baseUrl))
                {
                    ISite siteSettings;
                    using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
                    {
                        siteSettings = await scope.ServiceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync();
                    }

                    baseUrl = siteSettings.BaseUrl;
                    basePath = requestUrlPrefix;
                }

                if (string.IsNullOrEmpty(baseUrl))
                {
                    _notifier.Warning(T["Technikai hiba történt. Kérjük vegye fel a kapcsolatot az adminisztrátorral."]);

                    return RedirectToAction(nameof(MyCenterProfiles));
                }

                var nonce = await _dccmCrossLoginHandler.GenerateNonceAsync(dokiNetMember.MemberRightId);
                var uriBuilder = new UriBuilder(baseUrl)
                {
                    Path = basePath + "/OrganiMedCore.DiabetesCareCenterTenant/CenterProfile/DccmLogin"
                };

                uriBuilder.AppendQueryParams("h", nonce.ToString("N"));

                return Redirect(uriBuilder.ToString());
            }
            catch (UserHasNoMemberRightIdException)
            {
                return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.DeleteCenterProfile))
            {
                return Unauthorized();
            }

            await _centerProfileService.DeleteCenterProfileAsync(id);
            _notifier.Success(T["A szakellátóhely törlése sikeres volt."]);

            // TODO: handle tenant, notification, etc

            return RedirectToAction(nameof(Index));
        }

        [Route("szakellatohely/elozmenyek")]
        public async Task<IActionResult> History(CenterProfileHistoryViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ViewListOfCenterProfiles))
            {
                return Unauthorized();
            }

            if (viewModel == null || string.IsNullOrEmpty(viewModel.Id))
            {
                return NotFound();
            }

            var history = await _centerProfileService.GetCenterProfileVersionsAsync(viewModel.Id);
            if (!history.Any())
            {
                _notifier.Error(T["A szakellátóhelynek nincsenek akkreditációs előzményei."]);

                return RedirectToAction(nameof(Display), new { viewModel.Id });
            }

            if (string.IsNullOrEmpty(viewModel.HistoryId))
            {
                viewModel.HistoryId = history.OrderBy(x => x.PublishedUtc).Last().ContentItemVersionId;
            }

            var requestedVersion = history.FirstOrDefault(version => version.ContentItemVersionId == viewModel.HistoryId);
            if (requestedVersion == null)
            {
                return NotFound();
            }

            ViewData["IsHistoryView"] = true;
            ViewData["History"] = history;
            ViewData["SummaryDisplay"] = await _contentItemDisplayManager.BuildDisplayAsync(requestedVersion, this, "Summary");

            return View(viewModel);
        }

        [Route(CenterProfileNamedRoutes.CenterProfile_Display)]
        public async Task<IActionResult> Display(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ViewListOfCenterProfiles))
            {
                return Unauthorized();
            }

            try
            {
                ContentItem contentItem = null;
                if (await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ViewAllCenterProfiles))
                {
                    contentItem = await _centerProfileService.GetCenterProfileAsync(id);
                }
                else if (User.IsInRole(CenterPosts.TerritorialRapporteur))
                {
                    contentItem = await _centerProfileService.GetPermittedCenterProfileAsync(id);
                }
                else
                {
                    return Unauthorized();
                }

                if (contentItem == null)
                {
                    return NotFound();
                }

                var reviewAuthorization = await _centerProfileReviewService.GetAuthorizationResultAsync(
                    await _betterUserService.GetCurrentUserAsync(),
                    contentItem);

                ViewData["CurrentRole"] = reviewAuthorization.CurrentRole;
                ViewData["AuthorizedToReview"] = reviewAuthorization.IsAuthorized;
                ViewData["ContentItem"] = contentItem;
                ViewData["SummaryDisplay"] = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "Summary");

                return View(new CenterProfileReviewViewModel() { Id = id });
            }
            catch (SettlementNotFoundException ex)
            {
                _notifier.Error(T["A szakellátóhely címe érvénytelen."]);

                _logger.LogError(ex, "A szakellátóhely címe érvénytelen.");

                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedException)
            {
                _notifier.Error(T["Önnek nincs joga megtekinteni a szakellátóhely adatlapját."]);

                return RedirectToAction(nameof(Index));
            }
        }

        [ActionName(nameof(Display))]
        [HttpPost("szakellatohely-velemenyezes", Name = "CenterProfileReview")]
        public async Task<IActionResult> DisplayPost(CenterProfileReviewViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ReviewCenterProfile))
            {
                return Unauthorized();
            }

            try
            {
                var reviewCheckResult = await GetCenterProfileForReviewAsync(viewModel.Id);
                if (reviewCheckResult.ContentItem == null)
                {
                    return reviewCheckResult.Action;
                }

                ValidateReviewOutcome(viewModel);
                if (!ModelState.IsValid)
                {
                    ViewData["CurrentRole"] = reviewCheckResult.CurrentRole;
                    ViewData["ContentItem"] = reviewCheckResult.ContentItem;
                    ViewData["AuthorizedToReview"] = true;
                    ViewData["SummaryDisplay"] = await _contentItemDisplayManager.BuildDisplayAsync(reviewCheckResult.ContentItem, this, "Summary");

                    return View(viewModel);
                }

                await _centerProfileService.ReviewAsync(reviewCheckResult, viewModel.ReviewOutcome.Value, viewModel.RejectReason);

                _notifier.Success(T["A szakellátóhely adatlapjának véleményezése megtörtént."]);

                if (viewModel.ReviewOutcome.Value)
                {
                    await NotifyAcceptedCenterProfileAsync(reviewCheckResult);
                }
                else
                {
                    await NotifyRejectedCenterProfileAsync(reviewCheckResult, viewModel.RejectReason);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById");

                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Revisions(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ViewListOfCenterProfiles))
            {
                return Unauthorized();
            }

            var reviewCheckResult = await GetCenterProfileForReviewAsync(id);
            if (reviewCheckResult.ContentItem == null)
            {
                return reviewCheckResult.Action;
            }

            var part = reviewCheckResult.ContentItem.As<CenterProfileReviewStatesPart>();

            ViewData["Id"] = id;

            return View(part.States);
        }

        [HttpGet("szakellatohely/vezeto-levaltas/{id}")]
        public async Task<IActionResult> ChangeLeader(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterProfileLeaders))
            {
                return Unauthorized();
            }

            var memberRightId = 0;
            try
            {
                var contentItem = await _centerProfileService.GetCenterProfileAsync(id);
                if (contentItem == null)
                {
                    _notifier.Error(T["A szakellátóhely adatlapja nem található."]);

                    return RedirectToAction(nameof(Index));
                }

                var part = contentItem.As<CenterProfilePart>();
                memberRightId = part.MemberRightId;
                var dokiNetMemberLeader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(memberRightId);

                var viewModel = new ChangeLeaderViewModel()
                {
                    CenterProfileName = part.CenterName,
                    EditDokiNetMemberViewModel = new EditDokiNetMemberViewModel()
                };
                viewModel.EditDokiNetMemberViewModel.UpdateViewModel(dokiNetMemberLeader);

                return View(viewModel);
            }
            catch (HttpRequestException ex)
            {
                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", "MRID:" + memberRightId);

                return RedirectToAction(nameof(Index));
            }
        }

        [ActionName(nameof(ChangeLeader))]
        [HttpPost("szakellatohely/vezeto-levaltas/{id}", Name = nameof(ChangeLeaderPost))]
        public async Task<IActionResult> ChangeLeaderPost(string id, ChangeLeaderViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterProfileLeaders))
            {
                return Unauthorized();
            }

            try
            {
                ContentItem contentItem = null;
                CenterProfilePart part = null;
                if (!string.IsNullOrEmpty(id))
                {
                    contentItem = await _centerProfileService.GetCenterProfileAsync(id);
                    if (contentItem == null)
                    {
                        _notifier.Error(T["A szakellátóhely adatlapja nem található."]);

                        return RedirectToAction(nameof(Index));
                    }

                    part = contentItem.As<CenterProfilePart>();
                }

                if (ValidateChangeLeaderViewModel(contentItem, viewModel))
                {
                    var nextLeader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(
                        viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                    var assignedTenantName = contentItem.As<CenterProfileManagerExtensionsPart>().AssignedTenantName;
                    if (!string.IsNullOrEmpty(assignedTenantName))
                    {
                        // Change leader in the DCC tenant
                        using (var scope = await _sharedDataAccessorService.GetTenantServiceScopeAsync(assignedTenantName))
                        {
                            var service = scope.ServiceProvider.GetRequiredService<IDiabetesCareCenterService>();
                            await service.ChangeCenterProfileLeaderAsync(nextLeader, this);
                        }
                    }

                    // Change leader (MemberRightId property) in the DCCM tenant
                    await _centerProfileService.ChangeCenterProfileLeaderAsync(contentItem, viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                    await QueueNotificationAboutLeaderChanged(assignedTenantName, part, nextLeader);

                    _notifier.Success(T["A vezető leváltása megtörtént."]);

                    return RedirectToAction(nameof(Index));
                }

                viewModel.CenterProfileName = part?.CenterName;

                if (viewModel?.EditDokiNetMemberViewModel?.MemberRightId != null)
                {
                    var dokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(
                        viewModel.EditDokiNetMemberViewModel.MemberRightId.Value);

                    viewModel.EditDokiNetMemberViewModel.UpdateViewModel(dokiNetMember);
                }

                return View(viewModel);
            }
            catch (DokiNetMemberRegistrationException ex)
            {
                _notifier.Error(T[ex.Message]);

                _logger.LogError(ex, ex.Message);

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _notifier.Error(T["Hiba történt a vezető leváltása közben."]);

                _logger.LogError(ex, ex.Message);

                return RedirectToAction(nameof(Index));
            }
        }

        [Route("szakellatohely/ujraszamol/{id}", Name = "CenterProfile.RecalculateAccreditationStatus")]
        public async Task<IActionResult> RecalculateAccreditationStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ViewListOfCenterProfiles))
            {
                return Unauthorized();
            }

            var contentItem = await _centerProfileService.GetCenterProfileAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }

            await _centerProfileService.CalculateAccreditationStatusAsync(contentItem);

            return RedirectToAction(nameof(Display), new { id });
        }


        private async Task<IActionResult> MyCenterProfilesCommon(Func<bool, DokiNetMember, Task<IActionResult>> finalResult)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfile))
            {
                return RedirectWhenUserIsNotDoctor();
            }

            try
            {
                var dokiNetMember = await _sharedUserService.UpdateAndGetCurrentUsersDokiNetMemberDataAsync();
                if (dokiNetMember == null || dokiNetMember.MemberRightId == 0)
                {
                    return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
                }

                if (string.IsNullOrEmpty(dokiNetMember.StampNumber))
                {
                    return RedirectWhenUserIsNotDoctor();
                }

                return await finalResult(
                    await _diabetesUserProfileService.HasMissingQualificationsForOccupation(Occupation.Doctor, dokiNetMember),
                    dokiNetMember);
            }
            catch (UserHasNoMemberRightIdException)
            {
                return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById");

                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                return this.RedirectToHome();
            }
        }

        private async Task QueueNotificationAboutLeaderChanged(string assignedTenantName, CenterProfilePart part, DokiNetMember nextLeader)
        {
            var data = new CenterProfileLeaderChanged()
            {
                CenterName = part.CenterName,
                NewLeaderName = nextLeader.FullName
            };

            var emailNotification = new EmailNotification();

            if (!string.IsNullOrEmpty(assignedTenantName))
            {
                var nonce = new Nonce()
                {
                    RedirectUrl = Url.Action(nameof(MyCenterProfiles), "CenterProfile", new { area = "OrganiMedCore.DiabetesCareCenterManager" }),
                    Type = NonceType.MemberRightId,
                    TypeId = part.MemberRightId
                };

                await _nonceService.CreateAsync(nonce);
                data.Nonce = nonce.Value;

                emailNotification.To.Add(nextLeader.Emails.FirstOrDefault());
                emailNotification.TemplateId = EmailTemplateIds.CenterProfileLeaderChanged;
            }
            else
            {
                var emailSettings = await _emailSettingsService.GetEmailSettingsAsync();
                emailNotification.To = emailSettings.DebugEmailAddresses;
                emailNotification.TemplateId = EmailTemplateIds.CenterProfileLeaderChangedBeforeAssignment;
            }

            emailNotification.Data = data;

            await _emailNotificationDataService.QueueAsync(emailNotification);
        }

        private bool ValidateChangeLeaderViewModel(ContentItem contentItem, ChangeLeaderViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return false;
            }

            if (!viewModel.EditDokiNetMemberViewModel.MemberRightId.HasValue)
            {
                _notifier.Error(T["Az új vezető nem lett kiválasztva."]);

                return false;
            }

            if (contentItem.As<CenterProfilePart>().MemberRightId == viewModel.EditDokiNetMemberViewModel.MemberRightId)
            {
                _notifier.Error(T["Az új vezető megegyezik a régi vezetővel."]);

                return false;
            }

            return true;
        }

        private async Task<IActionResult> CenterProfilePartsResultAsync(IEnumerable<ContentItem> contentItems, string viewName)
        {
            var members = await _dokiNetService.GetDokiNetMembersByIds<DokiNetMember>(
                contentItems.Select(contentItem => contentItem.As<CenterProfilePart>().MemberRightId));

            var settlements = contentItems.Select(x =>
            {
                var part = x.As<CenterProfilePart>();

                return new Settlement()
                {
                    ZipCode = part.CenterZipCode,
                    Name = part.CenterSettlementName
                };
            });

            ViewData["ReviewableCenterProfileIds"] =
                (await _centerProfileReviewService.GetReviewerStatisticsAsync(
                    await _betterUserService.GetCurrentUserAsync(),
                    contentItems))
                .ReviewableContentItemIds;

            ViewData["CalculatedStatusOverridable"] = (await _centerProfileCommonService.GetCenterManagerSettingsAsync()).CalculatedStatusOverridable;

            return View(
                viewName,
                contentItems.Select(contentItem =>
                {
                    var member = members.FirstOrDefault(x => x.MemberRightId == contentItem.As<CenterProfilePart>().MemberRightId);

                    return CenterProfileComplexViewModel.CreateViewModel(contentItem, true, true, true, true, true, member);
                }));
        }

        private async Task<CenterProfileReviewCheckResult> GetCenterProfileForReviewAsync(string id)
        {
            var result = new CenterProfileReviewCheckResult();

            var contentItem = await _centerProfileService.GetCenterProfileAsync(id);
            if (contentItem == null)
            {
                _notifier.Error(T["A szakellátóhely adatlapja nem található."]);
                result.Action = RedirectToAction(nameof(Index));

                return result;
            }

            var currentStatus = contentItem.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus;
            if (!currentStatus.HasValue || currentStatus == CenterProfileStatus.Unsubmitted)
            {
                if (!currentStatus.HasValue)
                {
                    _notifier.Error(T["A szakellátóhely adatlapja jelenleg nem szerkeszthető."]);
                }
                else
                {
                    _notifier.Error(T["A szakellátóhely adatlapja jelenleg a szakellátóhely vezető szerkesztése alatt áll."]);
                }

                result.Action = RedirectToAction(nameof(Index));

                return result;
            }

            try
            {
                var reviewAuthorization = await _centerProfileReviewService.GetAuthorizationResultAsync(
                    await _betterUserService.GetCurrentUserAsync(),
                    contentItem);
                if (!reviewAuthorization.IsAuthorized)
                {
                    return UnauthorizedToReview();
                }

                result.CurrentRole = reviewAuthorization.CurrentRole;
                result.ContentItem = contentItem;

                return result;
            }
            catch (SettlementNotFoundException ex)
            {
                _notifier.Error(T["A szakellátóhely címe érvénytelen."]);

                _logger.LogError(ex, "A szakellátóhely címe érvénytelen.");
            }

            result.Action = RedirectToAction(nameof(Index));

            return result;
        }

        private CenterProfileReviewCheckResult UnauthorizedToReview()
        {
            _notifier.Error(T["Ön nem jogosult a szakellátóhely adatlapjának véleményezésére."]);

            return new CenterProfileReviewCheckResult()
            {
                Action = RedirectToAction(nameof(Index))
            };
        }

        private void ValidateReviewOutcome(CenterProfileReviewViewModel viewModel)
        {
            if (!viewModel.ReviewOutcome.HasValue)
            {
                ModelState.AddModelError(string.Empty, T["A döntés nem lett kiválasztva."].Value);
            }

            if (viewModel.ReviewOutcome == false && string.IsNullOrEmpty(viewModel.RejectReason))
            {
                ModelState.AddModelError(string.Empty, T["Elutasítás esetén meg kell indokolni a döntést."].Value);
            }
        }

        private async Task NotifyAcceptedCenterProfileAsync(CenterProfileReviewCheckResult reviewCheckResult)
        {
            await EmailNotificationExtensions.NotifyAcceptedCenterProfileAsync(
                reviewCheckResult.ContentItem,
                reviewCheckResult.CurrentRole,
                reviewCheckResult.ContentItem.As<CenterProfileManagerExtensionsPart>().AssignedTenantName,
                _dokiNetService,
                _sharedUserService,
                _territoryService,
                _userManager,
                _emailNotificationDataService.QueueAsync);
        }

        private async Task NotifyRejectedCenterProfileAsync(CenterProfileReviewCheckResult reviewCheckResult, string rejectReason)
        {
            var part = reviewCheckResult.ContentItem.As<CenterProfilePart>();

            var notifiedUsers = new List<IUser>();

            if (reviewCheckResult.CurrentRole == CenterPosts.OMKB ||
                reviewCheckResult.CurrentRole == CenterPosts.MDTManagement)
            {
                var reviewers = await _territoryService.GetReviewersAsync(part.CenterZipCode, part.CenterSettlementName);
                if (reviewers != null)
                {
                    notifiedUsers.AddRange(reviewers);
                }
            }

            if (reviewCheckResult.CurrentRole == CenterPosts.MDTManagement)
            {
                notifiedUsers.AddRange(await _userManager.GetUsersInRoleAsync(CenterPosts.OMKB));
            }


            var leaderMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(part.MemberRightId);
            var notifiedDokiNetMembers = new List<DokiNetMember>() { leaderMember };

            var userNames = EmailNotificationExtensions.GetDistinctUserNames(notifiedUsers);
            notifiedDokiNetMembers.AddRange(await _sharedUserService.GetDokiNetMembersFromManagersScopeByLocalUserIdsAsync(userNames));

            notifiedDokiNetMembers = EmailNotificationExtensions.GetDistinctDokiNetMembers(notifiedDokiNetMembers);

            foreach (var dokiNetMember in notifiedDokiNetMembers)
            {
                var nonce = new Nonce()
                {
                    Type = NonceType.MemberRightId,
                    TypeId = dokiNetMember.MemberRightId,
                    RedirectUrl = dokiNetMember.MemberRightId == leaderMember.MemberRightId
                        ? Url.Action(
                            "MyCenterProfiles",
                            "CenterProfile",
                            new { area = "OrganiMedCore.DiabetesCareCenterManager" })
                        : Url.Action(
                            "Display",
                            "CenterProfile",
                            new { area = "OrganiMedCore.DiabetesCareCenterManager", id = reviewCheckResult.ContentItem.ContentItemId })
                };
                await _nonceService.CreateAsync(nonce);

                await _emailNotificationDataService.QueueAsync(new EmailNotification()
                {
                    Data = new CenterProfileRejectedViewModel()
                    {
                        PersonName = dokiNetMember.FullName,
                        RejectReason = rejectReason,
                        CenterName = part.CenterName,
                        CurrentRole = reviewCheckResult.CurrentRole,
                        Nonce = nonce.Value
                    },
                    TemplateId = EmailTemplateIds.CenterProfileRejected,
                    To = new HashSet<string>(new[] { dokiNetMember.Emails.FirstOrDefault() })
                });
            }
        }

        private IActionResult RedirectWhenUserIsNotDoctor()
        {
            _notifier.Error(T["Önnek nincs joga elérni az kívánt menüpontot."]);

            return this.RedirectToHome();
        }
    }
}
