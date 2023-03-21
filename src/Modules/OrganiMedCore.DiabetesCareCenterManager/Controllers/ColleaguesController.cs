using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.DiabetesCareCenterManager.Exceptions;
using OrganiMedCore.DiabetesCareCenterManager.Extensions;
using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Login.Constants;
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
    // TODO: jelentkezést/visszavonást service-be kiszervezni
    public class ColleaguesController : Controller, IUpdateModel
    {
        private readonly ICenterProfileService _centerProfileService;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IDiabetesUserProfileService _diabetesUserProfileService;
        private readonly IDokiNetService _dokiNetService;
        private readonly IEmailNotificationDataService _emailNotificationDataService;
        private readonly ILogger _logger;
        private readonly INonceService _nonceService;
        private readonly INotifier _notifier;
        private readonly ISharedUserService _sharedUserService;


        public IHtmlLocalizer T { get; set; }


        public ColleaguesController(
            ICenterProfileService centerProfileService,
            IClock clock,
            IContentManager contentManager,
            IDiabetesUserProfileService diabetesUserProfileService,
            IDokiNetService dokiNetService,
            IEmailNotificationDataService emailNotificationDataService,
            IHtmlLocalizer<ColleaguesController> htmlLocalizer,
            ILogger<ColleaguesController> logger,
            INonceService nonceService,
            INotifier notifier,
            ISharedUserService sharedUserService)
        {
            _centerProfileService = centerProfileService;
            _clock = clock;
            _contentManager = contentManager;
            _diabetesUserProfileService = diabetesUserProfileService;
            _dokiNetService = dokiNetService;
            _emailNotificationDataService = emailNotificationDataService;
            _logger = logger;
            _nonceService = nonceService;
            _notifier = notifier;
            _sharedUserService = sharedUserService;

            T = htmlLocalizer;
        }


        [Route(CenterProfileNamedRoutes.CenterProfile_Colleagues)]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized();
                }

                ViewData["DokiNetMember"] = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();

                var centerProfileContentItems = await _centerProfileService.GetCenterProfilesAsync();
                var leaderDokiNetMembers = await _dokiNetService.GetDokiNetMembersByIds<DokiNetMember>(
                    centerProfileContentItems.Select(contentItem => contentItem.As<CenterProfilePart>().MemberRightId));

                return View(centerProfileContentItems.Select(contentItem =>
                    CenterProfileComplexViewModel.CreateViewModel(
                        contentItem: contentItem,
                        basicData: true,
                        renewal: true,
                        colleagues: true,
                        member: leaderDokiNetMembers.FirstOrDefault(m => m.MemberRightId == contentItem.As<CenterProfilePart>().MemberRightId))));
            }
            catch (UserHasNoMemberRightIdException)
            {
                return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
            }
            catch (MemberNotFoundException)
            {
                // TODO
                return Unauthorized();
            }
            catch (DokiNetMemberRegistrationException ex)
            {
                _notifier.Error(T[ex.Message]);

                return Unauthorized();
            }
            catch (HttpRequestException ex)
            {
                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                _logger.LogError(ex, "DokiNetService.GetDokiNetMembersByIds");

                return this.RedirectToHome();
            }
        }

        [Route(CenterProfileNamedRoutes.CenterProfile_Colleagues_Apply + "{id}")]
        public async Task<IActionResult> Apply(string id)
        {
            var leaderMemberRightId = 0;
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return LocalRedirect($"~/{NamedRoutes.DokiNetLoginPath}?returnUrl=" + Url.Action(nameof(Apply), "Colleagues", new { id }));
                }

                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest();
                }

                var contentItem = await _centerProfileService.GetCenterProfileAsync(id);
                if (contentItem == null)
                {
                    return NotFound();
                }

                if (CenterProfileApplicationDisabled(contentItem))
                {
                    return RedirectToAction(nameof(Index));
                }

                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
                leaderMemberRightId = contentItem.As<CenterProfilePart>().MemberRightId;
                if (dokiNetMember.MemberRightId == leaderMemberRightId)
                {
                    return RedirectOnLeaderApplication();
                }

                var (colleague, occupationCanBeChanged) = GetColleagueForApplication(contentItem, dokiNetMember.MemberRightId);
                var leaderDokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(leaderMemberRightId);

                InitViewDataToApply(contentItem, dokiNetMember, leaderDokiNetMember, occupationCanBeChanged);

                var viewModel = new CenterProfileApplicationViewModel()
                {
                    ContentItemId = contentItem.ContentItemId,
                    Occupation = colleague?.Occupation
                };

                return View(viewModel);
            }
            catch (ColleagueAlreadyJoinedException)
            {
                return ColleagueAlreadyJoinedResult();
            }
            catch (UserHasNoMemberRightIdException)
            {
                return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
            }
            catch (MemberNotFoundException)
            {
                return Unauthorized();
            }
            catch (DokiNetMemberRegistrationException ex)
            {
                _notifier.Error(T[ex.Message]);

                return Unauthorized();
            }
            catch (HttpRequestException ex)
            {
                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", "MRID:" + leaderMemberRightId);

                return this.RedirectToHome();
            }
        }

        [ActionName(nameof(Apply))]
        [HttpPost("munkahely/megerosit", Name = "DccmColleagueApplyPost")]
        public async Task<IActionResult> ApplyPost(CenterProfileApplicationViewModel viewModel)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var contentItem = await _centerProfileService.GetCenterProfileAsync(viewModel.ContentItemId);
            if (contentItem == null)
            {
                return NotFound();
            }

            if (CenterProfileApplicationDisabled(contentItem))
            {
                return RedirectToAction(nameof(Index));
            }

            var leaderMemberRightId = 0;
            try
            {
                var occupationCanChange = true;
                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
                var part = contentItem.As<CenterProfilePart>();

                if (ModelState.IsValid)
                {
                    if (dokiNetMember.MemberRightId == part.MemberRightId)
                    {
                        return RedirectOnLeaderApplication();
                    }

                    var (colleague, occupationCanBeChanged) = GetColleagueForApplication(contentItem, dokiNetMember.MemberRightId);
                    if (!occupationCanBeChanged && viewModel.Occupation != colleague.Occupation)
                    {
                        throw new ColleagueCannotChangeOccupationException();
                    }

                    if (await _diabetesUserProfileService.HasMissingQualificationsForOccupation(viewModel.Occupation.Value, dokiNetMember))
                    {
                        throw new ColleagueHasNoRequiredQualificationsException();
                    }

                    occupationCanChange = occupationCanBeChanged;

                    var isNew = colleague == null;
                    if (isNew)
                    {
                        colleague = new Colleague()
                        {
                            Id = Guid.NewGuid()
                        };
                    }

                    var invited = colleague.LatestStatusItem?.Status == ColleagueStatus.Invited;

                    colleague.StatusHistory.Add(new ColleagueStatusItem()
                    {
                        StatusDateUtc = _clock.UtcNow,
                        Status = invited ? ColleagueStatus.InvitationAccepted : ColleagueStatus.ApplicationSubmitted
                    });

                    colleague.FirstName = dokiNetMember.FirstName;
                    colleague.LastName = dokiNetMember.LastName;
                    colleague.Email = dokiNetMember.Emails.FirstOrDefault();
                    colleague.Prefix = dokiNetMember.Prefix;
                    colleague.MemberRightId = dokiNetMember.MemberRightId;
                    colleague.CenterProfileContentItemId = contentItem.ContentItemId;
                    colleague.CenterProfileContentItemVersionId = contentItem.ContentItemVersionId;
                    colleague.Occupation = viewModel.Occupation;

                    await UpdateColleagueAsync(contentItem, colleague);

                    await NotifyActionStartedByColleagueAsync(contentItem, colleague, invited ? ColleagueStatus.InvitationAccepted : ColleagueStatus.ApplicationSubmitted);

                    _notifier.Success(T["Jelentkezése a szakellátóhelyre sikeresen megtörtént."]);

                    return RedirectToAction(nameof(Index));
                }

                leaderMemberRightId = part.MemberRightId;
                var leaderDokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(leaderMemberRightId);

                InitViewDataToApply(contentItem, dokiNetMember, leaderDokiNetMember, occupationCanChange);

                return View(viewModel);
            }
            catch (ColleagueAlreadyJoinedException)
            {
                return ColleagueAlreadyJoinedResult();
            }
            catch (UserHasNoMemberRightIdException)
            {
                return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
            }
            catch (ColleagueCannotChangeOccupationException)
            {
                _notifier.Error(T["A munkakör nem változtatható meg."]);

                return RedirectToAction(nameof(Apply), new { id = viewModel.ContentItemId });
            }
            catch (ColleagueHasNoRequiredQualificationsException)
            {
                _notifier.Error(T["Az Ön képesítései alapján nem lehetséges a jelentkezés az adott munkakörre."]);

                return RedirectToAction(nameof(Apply), new { id = viewModel.ContentItemId });
            }
            catch (HttpRequestException ex)
            {
                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", "MRID:" + leaderMemberRightId);

                return this.RedirectToHome();
            }

        }

        [Route(CenterProfileNamedRoutes.CenterProfile_Colleagues_Cancel + "{id}/{sure?}")]
        public async Task<IActionResult> Cancel(string id, bool sure = false)
        {
            var leaderMemberRightId = 0;
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return LocalRedirect($"~/{NamedRoutes.DokiNetLoginPath}?returnUrl=" + Url.Action(nameof(Cancel), "Colleagues", new { id }));
                }

                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest();
                }

                var contentItem = await _centerProfileService.GetCenterProfileAsync(id);
                if (contentItem == null)
                {
                    return NotFound();
                }

                if (CenterProfileApplicationDisabled(contentItem))
                {
                    return RedirectToAction(nameof(Index));
                }

                var centerProfilePart = contentItem.As<CenterProfilePart>();
                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
                leaderMemberRightId = centerProfilePart.MemberRightId;
                if (dokiNetMember.MemberRightId == leaderMemberRightId)
                {
                    return RedirectOnLeaderApplication();
                }

                var colleague = centerProfilePart.Colleagues.FirstOrDefault(x => x.MemberRightId == dokiNetMember.MemberRightId);
                if (colleague == null || !ColleagueStatusExtensions.AllowedStatusesToCancel.Contains(colleague.LatestStatusItem.Status))
                {
                    _notifier.Warning(T["A kérés nem hajtható végre"]);

                    return RedirectToAction(nameof(Index));
                }

                if (!sure)
                {
                    var leaderDokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(leaderMemberRightId);
                    ViewData["CenterProfile"] = GetCenterProfilePartViewModel(contentItem, leaderDokiNetMember);

                    return View();
                }

                ColleagueStatus newStatus;
                switch (colleague.LatestStatusItem.Status)
                {
                    case ColleagueStatus.ApplicationSubmitted:
                        newStatus = ColleagueStatus.ApplicationCancelled;
                        break;
                    case ColleagueStatus.Invited:
                        newStatus = ColleagueStatus.InvitationRejected;
                        break;

                    case ColleagueStatus.ApplicationAccepted:
                    case ColleagueStatus.PreExisting:
                    case ColleagueStatus.InvitationAccepted:
                    default:
                        newStatus = ColleagueStatus.DeletedByColleague;
                        break;
                }

                var invited = colleague.LatestStatusItem?.Status == ColleagueStatus.Invited;

                colleague.StatusHistory.Add(new ColleagueStatusItem()
                {
                    StatusDateUtc = _clock.UtcNow,
                    Status = newStatus
                });

                await UpdateColleagueAsync(contentItem, colleague);

                await NotifyActionStartedByColleagueAsync(contentItem, colleague, invited ? ColleagueStatus.InvitationRejected : ColleagueStatus.ApplicationCancelled);

                _notifier.Success(T["Ön sikeresen visszavonta jelentkezését."]);

                return RedirectToAction(nameof(Index));
            }
            catch (UserHasNoMemberRightIdException)
            {
                return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
            }
            catch (MemberNotFoundException)
            {
                return Unauthorized();
            }
            catch (DokiNetMemberRegistrationException ex)
            {
                _notifier.Error(T[ex.Message]);

                return Unauthorized();
            }
            catch (HttpRequestException ex)
            {
                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", "MRID:" + leaderMemberRightId);

                return this.RedirectToHome();
            }
        }


        private CenterProfileComplexViewModel GetCenterProfilePartViewModel(ContentItem contentItem, DokiNetMember member)
            => CenterProfileComplexViewModel.CreateViewModel(
                    contentItem: contentItem,
                    basicData: true,
                    renewal: true,
                    member: member);

        private bool CenterProfileApplicationDisabled(ContentItem contentItem)
        {
            if (!contentItem.As<CenterProfileManagerExtensionsPart>().ApplicationEnabled())
            {
                _notifier.Error(T["A szakellátóhely akkreditációja folyamatban van. A kérelem végrehajtása jelenleg nem lehetséges."]);

                return true;
            }

            return false;
        }

        private IActionResult ColleagueAlreadyJoinedResult()
        {
            _notifier.Error(T["Ön már korábban jelentkezett a választott szakellátóhelyre."]);

            return RedirectToAction(nameof(Index));
        }

        private (Colleague, bool) GetColleagueForApplication(ContentItem contentItem, int memberRightId)
        {
            var part = contentItem.As<CenterProfilePart>();
            var colleague = part.Colleagues.FirstOrDefault(x => x.MemberRightId == memberRightId);
            if (colleague != null && !ColleagueStatusExtensions.AllowedStatusesToApply.Contains(colleague.LatestStatusItem.Status))
            {
                throw new ColleagueAlreadyJoinedException();
            }

            var occupationCanBeChanged = colleague?.LatestStatusItem.Status != ColleagueStatus.Invited;

            return (colleague, occupationCanBeChanged);
        }

        private IActionResult RedirectOnLeaderApplication()
        {
            _notifier.Warning(T["Ön a szakellátóhely vezetője. A jelentkezés nem lehetséges."]);

            return RedirectToAction(nameof(Index));
        }

        private async Task UpdateColleagueAsync(ContentItem contentItem, Colleague colleague)
        {
            contentItem.Alter<CenterProfilePart>(part =>
            {
                part.Colleagues = part.Colleagues.Where(x => x.MemberRightId != colleague.MemberRightId).ToList();
                part.Colleagues.Add(colleague);
            });

            // Will update the contentItem.
            await _centerProfileService.CalculateAccreditationStatusAsync(contentItem);
        }

        private void InitViewDataToApply(
            ContentItem contentItem,
            DokiNetMember dokiNetMember,
            DokiNetMember leaderDokiNetMember,
            bool occupationCanBeChanged)
        {
            ViewData["OccupationCanBeChanged"] = occupationCanBeChanged;
            ViewData["Emails"] = dokiNetMember.Emails;
            ViewData["CenterProfile"] = GetCenterProfilePartViewModel(contentItem, leaderDokiNetMember);
        }

        private async Task NotifyActionStartedByColleagueAsync(ContentItem contentItem, Colleague colleague, ColleagueStatus colleagueStatus)
        {
            var part = contentItem.As<CenterProfilePart>();
            var leader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(part.MemberRightId);

            var templateId = string.Empty;
            switch (colleagueStatus)
            {
                case ColleagueStatus.InvitationAccepted:
                    templateId = EmailTemplateIds.ColleagueAction_InvitationAccepted;
                    break;
                case ColleagueStatus.InvitationRejected:
                    templateId = EmailTemplateIds.ColleagueAction_InvitationRejected;
                    break;
                case ColleagueStatus.ApplicationSubmitted:
                    templateId = EmailTemplateIds.ColleagueAction_ApplicationSubmitted;
                    break;
                case ColleagueStatus.ApplicationCancelled:
                    templateId = EmailTemplateIds.ColleagueAction_ApplicationCancelled;
                    break;
                default:
                    return;
            }

            var nonce = new Nonce()
            {
                // TODO: Forward maybe?? contentItem.ContentItemId?
                RedirectUrl = Url.Action("MyCenterProfiles", "CenterProfile", new { area = "OrganiMedCore.DiabetesCareCenterManager" }),
                Type = NonceType.MemberRightId,
                TypeId = part.MemberRightId
            };
            await _nonceService.CreateAsync(nonce);

            await _emailNotificationDataService.QueueAsync(new EmailNotification()
            {
                TemplateId = templateId,
                To = new HashSet<string>() { leader.Emails.First() },
                Data = new CenterProfileColleagueActionNotificationViewModel()
                {
                    CenterName = part.CenterName,
                    ColleagueName = colleague.FullName,
                    LeaderName = leader.FullName,
                    Occupation = colleague.Occupation,
                    Nonce = nonce.Value
                }
            });
        }
    }
}
