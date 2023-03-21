using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.DiabetesCareCenterTenant.Extensions;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Services;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterTenant.Controllers.Api
{
    [Authorize, Route("api/center-profile-local", Name = "CenterProfileLocalApi")]
    public class CenterProfileApiController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICenterProfileManagerService _centerProfileManagerService;
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISharedUserService _sharedUserService;


        public IHtmlLocalizer T { get; set; }


        public CenterProfileApiController(
            IAuthorizationService authorizationService,
            ICenterProfileManagerService centerProfileManagerService,
            IDokiNetService dokiNetService,
            IHtmlLocalizer<CenterProfileApiController> htmlLocalizer,
            ILogger<CenterProfileApiController> logger,
            ISharedDataAccessorService sharedDataAccessorService,
            ISharedUserService sharedUserService)
        {
            _authorizationService = authorizationService;
            _centerProfileManagerService = centerProfileManagerService;
            _dokiNetService = dokiNetService;
            _logger = logger;
            _sharedDataAccessorService = sharedDataAccessorService;
            _sharedUserService = sharedUserService;

            T = htmlLocalizer;
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery(Name = "s")]CenterProfileEditorStep step)
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

                if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
                {
                    return BadRequest();
                }

                ICenterProfileViewModel viewModel = null;
                object additionalData = null;
                switch (step)
                {
                    case CenterProfileEditorStep.BasicData:
                        var leader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(contentItem.As<CenterProfilePart>().MemberRightId);
                        additionalData = CenterProfileHelpers.GetBasicAdditionalData(leader);
                        viewModel = new CenterProfileBasicDataViewModel();
                        break;

                    case CenterProfileEditorStep.AdditionalData:
                        viewModel = new CenterProfileAdditionalDataViewModel();
                        break;

                    case CenterProfileEditorStep.Equipments:
                        viewModel = new CenterProfileEquipmentsViewModel();
                        break;

                    case CenterProfileEditorStep.Colleagues:
                        viewModel = new CenterProfileColleaguesViewModel();
                        break;

                    default:
                        return BadRequest();
                }

                viewModel.UpdateViewModel(contentItem.As<CenterProfilePart>());

                if (additionalData == null)
                {
                    return Ok(new { viewModel });
                }

                return Ok(new { additionalData, viewModel });
            }
            catch (CenterProfileNotAssignedException)
            {
                return NotFound();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "CenterProfileManagerService.GetDokiNetMemberById");

                return Conflict(T["Hiba a társasági rendszerrel történő kapcsolat közben."].Value);
            }
        }

        [HttpGet]
        [Route("get-status", Name = "CenterProfileLocalApi.GetAccreditationStatus")]
        public async Task<IActionResult> GetAccreditationStatus()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageCenterProfile))
            {
                return Unauthorized();
            }

            var contentItem = await _centerProfileManagerService.GetCenterProfileEditorForCurrentCenterAsync();
            if (!await _sharedUserService.AuthorizedToViewCenterProfile(contentItem))
            {
                return Unauthorized();
            }

            if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
            {
                return BadRequest();
            }

            var accreditationStatusResult = contentItem.As<CenterProfileManagerExtensionsPart>().AccreditationStatusResult
                ?? new AccreditationStatusResult();

            return Ok(new
            {
                accreditationStatusResult
            });
        }

        [HttpGet, Route("summary", Name = "CenterProfileLocalApi.Summary")]
        public async Task<IActionResult> GetSummary()
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

                if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
                {
                    return BadRequest();
                }

                var leader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(contentItem.As<CenterProfilePart>().MemberRightId);

                return Ok(new
                {
                    viewModel = CenterProfileComplexViewModel.CreateViewModel(contentItem, true, true, true, true, true, leader)
                });
            }
            catch (CenterProfileNotAssignedException)
            {
                return NotFound();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "CenterProfileManagerService.GetDokiNetMemberById");

                return Conflict(T["Hiba a társasági rendszerrel történő kapcsolat közben."].Value);
            }
        }

        [HttpGet, Route("search-by-zipcode", Name = "CenterProfileLocalApi.SearchByZipCode")]
        public async Task<IActionResult> Get(int zipCode)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageCenterProfile))
            {
                return Unauthorized();
            }

            return Ok(await _centerProfileManagerService.SearchTerritoryByZipCode(zipCode));
        }

        [HttpPost, Route("basic-data", Name = "CenterProfileLocalApi.BasicData")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post([FromBody]CenterProfileBasicDataViewModel viewModel)
            => await SaveCenterProfileWithoutSubmitAsync(viewModel);

        [HttpPost, Route("additional-data", Name = "CenterProfileLocalApi.AdditionalData")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post([FromBody]CenterProfileAdditionalDataViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationErrors = await CenterProfileHelpers.ValidateAdditionalDataAsync(viewModel);
            if (validationErrors.Any())
            {
                foreach (var error in validationErrors.SelectMany(item => item.Value))
                {
                    ModelState.AddModelError(string.Empty, T[error].Value);
                }

                return BadRequest(ModelState);
            }

            return await SaveCenterProfileWithoutSubmitAsync(viewModel);
        }

        [HttpPost, Route("equipments", Name = "CenterProfileLocalApi.Equipments")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post([FromBody]CenterProfileEquipmentsViewModel viewModel)
            => await SaveCenterProfileWithoutSubmitAsync(viewModel);

        [HttpPost, Route("invite", Name = "CenterProfileLocalApi.ColleagueInvitation")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post([FromBody]CenterProfileColleagueViewModel viewModel)
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

                if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
                {
                    return BadRequest();
                }

                if (!ValidateColleague(viewModel))
                {
                    return BadRequest();
                }

                if (viewModel.MemberRightId == contentItem.As<CenterProfilePart>().MemberRightId)
                {
                    return BadRequest(T["A szakellátóhely vezető nem hívható meg."].Value);
                }

                var colleague = await _centerProfileManagerService.InviteColleagueAsync(contentItem, viewModel);

                if (colleague.MemberRightId.HasValue)
                {
                    await QueueInvitationToMemberEmailAsync(contentItem, colleague);
                }
                else
                {
                    await QueueInvitationEmailToNonMemberAsync(contentItem, colleague);
                }

                return Ok(colleague);
            }
            catch (FormatException)
            {
                return BadRequest(T["A munkatárs nem található."].Value);
            }
            catch (ColleagueAlreadyExistsException)
            {
                return BadRequest(T["A kiválasztott tag már szerepel a listák egyikében."].Value);
            }
            catch (ColleagueEmailAlreadyTakenException)
            {
                return BadRequest(T["A megadott email használatban van egy másik munkatársnál."].Value);
            }
            catch (ColleagueException ex)
            {
                return BadRequest(T[ex.Message]);
            }
            catch (CenterProfileNotAssignedException)
            {
                return NotFound();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "CenterProfileManagerService.InviteColleagueAsync", "MRID:" + viewModel.MemberRightId.Value);

                return BadRequest(T["Hiba a társasági rendszerrel történő kapcsolat közben."].Value);
            }
        }

        [HttpGet, Route("get-colleague", Name = "CenterProfileLocalApi.GetColleagueAsDokiNetMember")]
        public async Task<IActionResult> GetColleagueAsDokiNetMember([FromQuery(Name = "id")]int memberRightId)
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

                if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
                {
                    return BadRequest();
                }

                if (!contentItem.As<CenterProfilePart>().Colleagues.Any(x => x.MemberRightId.HasValue))
                {
                    throw new ColleagueNotFoundException();
                }

                return Ok(await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(memberRightId));
            }
            catch (MemberNotFoundException)
            {
                return NotFound(T["A megadott munkatárs nem található"].Value);
            }
            catch (ColleagueNotFoundException)
            {
                return NotFound(T["A megadott munkatárs nem található"].Value);
            }
            catch (ColleagueException ex)
            {
                return BadRequest(T[ex.Message].Value);
            }
            catch (CenterProfileNotAssignedException)
            {
                return NotFound();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "CenterProfileManagerService.GetDokiNetMemberById", "MRID:" + memberRightId);

                return Conflict(T["Hiba a társasági rendszerrel történő kapcsolat közben."].Value);
            }
        }

        [HttpPut, Route("colleague", Name = "CenterProfileLocalApi.ColleagueAction")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Put([FromBody]CenterProfileColleagueActionViewModel viewModel)
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

                if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
                {
                    return BadRequest();
                }

                var colleague = await _centerProfileManagerService.ExecuteColleagueActionAsync(contentItem, viewModel);

                if (colleague.MemberRightId.HasValue)
                {
                    await NotifyColleagueActionToMemberAsync(contentItem, colleague, viewModel.ColleagueAction);
                }
                else
                {
                    await NotifyColleagueActionToNonMemberAsync(contentItem, colleague, viewModel.ColleagueAction);
                }

                return Ok(colleague.LatestStatusItem);
            }
            catch (ColleagueNotFoundException)
            {
                return NotFound(T["A megadott munkatárs nem található"].Value);
            }
            catch (ColleagueException ex)
            {
                return BadRequest(T[ex.Message].Value);
            }
            catch (CenterProfileNotAssignedException)
            {
                return NotFound();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, ex.Message);

                return BadRequest(T["Hiba történt a társasági rendszerrel történő kapcsolat során."].Value);
            }
        }

        [HttpGet, Route("submit", Name = "CenterProfileLocalApi.Submit")]
        public async Task<IActionResult> Get()
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

                if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
                {
                    return BadRequest();
                }

                await _centerProfileManagerService.SubmitCenterProfileAsync(contentItem);

                await NotifySubmittedCenterProfileAsync(contentItem);

                return Ok();
            }
            catch (CenterProfileNotAssignedException)
            {
                return NotFound();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "CenterProfileManagerService.GetDokiNetMemberById");

                return Conflict(T["Hiba a társasági rendszerrel történő kapcsolat közben."].Value);
            }
        }

        [HttpGet, Route("view-colleague-profile", Name = "CenterProfileLocalApi.ViewColleagueProfile")]
        public async Task<IActionResult> GetColleagueProfile(Guid id)
        {
            var contentItem = await _centerProfileManagerService.GetCenterProfileForCurrentCenterAsync();
            if (!await _sharedUserService.AuthorizedToViewCenterProfile(contentItem))
            {
                return Unauthorized();
            }

            var colleague = contentItem.As<CenterProfilePart>().Colleagues.FirstOrDefault(x => x.Id == id);
            if (colleague == null || !colleague.MemberRightId.HasValue)
            {
                return NotFound();
            }

            return Ok(await _centerProfileManagerService.GetPersonDataCompactViewModelAsync(colleague.MemberRightId.Value));
        }

        [HttpGet, Route("view-leader-profile", Name = "CenterProfileLocalApi.ViewLeaderProfile")]
        public async Task<IActionResult> GetColleagueProfile(int memberRightId)
        {
            var contentItem = await _centerProfileManagerService.GetCenterProfileForCurrentCenterAsync();
            if (!await _sharedUserService.AuthorizedToViewCenterProfile(contentItem))
            {
                return Unauthorized();
            }

            if (contentItem.As<CenterProfilePart>().MemberRightId != memberRightId)
            {
                return BadRequest();
            }

            return Ok(await _centerProfileManagerService.GetPersonDataCompactViewModelAsync(memberRightId));
        }


        private async Task<IActionResult> SaveCenterProfileWithoutSubmitAsync(ICenterProfileViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageCenterProfile))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var contentItem = await _centerProfileManagerService.GetCenterProfileEditorForCurrentCenterAsync();
                if (!await _sharedUserService.AuthorizedToViewCenterProfile(contentItem))
                {
                    return Unauthorized();
                }

                await _centerProfileManagerService.SaveCenterProfileAsync(viewModel);

                return Ok();
            }
            catch (CenterProfileNotAssignedException)
            {
                return NotFound();
            }
        }

        private bool ValidateColleague(CenterProfileColleagueViewModel viewModel)
        {
            if (!viewModel.Email.IsEmail())
            {
                return false;
            }

            if (!viewModel.MemberRightId.HasValue)
            {
                if (string.IsNullOrEmpty(viewModel.FirstName))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(viewModel.LastName))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task NotifySubmittedCenterProfileAsync(ContentItem centerProfileContentItem)
        {
            var part = centerProfileContentItem.As<CenterProfilePart>();

            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                var reviewers = await scope.ServiceProvider.GetRequiredService<ITerritoryService>()
                    .GetReviewersAsync(part.CenterZipCode, part.CenterSettlementName);

                var nonceService = scope.ServiceProvider.GetRequiredService<INonceService>();

                if (!reviewers.Any())
                {
                    return;
                }

                var managerSettings = await scope.ServiceProvider.GetRequiredService<ICenterProfileCommonService>()
                    .GetCenterManagerSettingsAsync();

                var leaderMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(part.MemberRightId);
                var reviewerMembers = await _sharedUserService.GetDokiNetMembersFromManagersScopeByLocalUserIdsAsync(reviewers.Select(x => x.UserName));

                var emailNotificationDataService = scope.ServiceProvider.GetRequiredService<IEmailNotificationDataService>();

                foreach (var reviewerUser in reviewerMembers)
                {
                    if (!reviewerUser.Emails.Any())
                    {
                        continue;
                    }

                    var nonce = new Nonce()
                    {
                        Type = NonceType.MemberRightId,
                        TypeId = reviewerUser.MemberRightId,
                        RedirectUrl = "~/" + CenterProfileNamedRoutes.CenterProfile_Display + "?id=" + centerProfileContentItem.ContentItemId
                    };
                    await nonceService.CreateAsync(nonce);

                    await emailNotificationDataService.QueueAsync(new EmailNotification()
                    {
                        TemplateId = EmailTemplateIds.CenterProfileSubmission,
                        Data = new CenterProfileSubmissionViewModel()
                        {
                            CenterLeaderName = leaderMember.FullName,
                            CenterName = part.CenterName,
                            ReviewerMemberFullName = reviewerUser.FullName,
                            Nonce = nonce.Value
                        },
                        To = new HashSet<string>(new[] { reviewerUser.Emails.FirstOrDefault() })
                    });
                }

                if (leaderMember.Emails.Any(x => x.IsEmail()))
                {
                    await emailNotificationDataService.QueueAsync(new EmailNotification()
                    {
                        TemplateId = EmailTemplateIds.CenterProfileSubmissionFeedback,
                        Data = new CenterProfileSubmissionFeedbackViewModel()
                        {
                            CenterLeaderName = leaderMember.FullName,
                            CenterName = part.CenterName,
                            Reviewers = reviewerMembers.Select(x => x.FullName).ToArray()
                        },
                        To = new HashSet<string>() { leaderMember.Emails.First(x => x.IsEmail()) }
                    });
                }
            }
        }

        private async Task NotifyColleagueActionToMemberAsync(ContentItem contentItem, Colleague colleague, ColleagueAction colleagueAction)
        {
            var templateId = string.Empty;
            switch (colleagueAction)
            {
                case ColleagueAction.RemoveActive:
                    templateId = EmailTemplateIds.ColleagueAction_ByLeader_RemoveActive;
                    break;
                case ColleagueAction.AcceptApplication:
                    templateId = EmailTemplateIds.ColleagueAction_ByLeader_AcceptApplication;
                    break;
                case ColleagueAction.RejectApplication:
                    templateId = EmailTemplateIds.ColleagueAction_ByLeader_RejectApplication;
                    break;
                //case ColleagueAction.ResendInvitation:
                //    templateId = EmailTemplateIds.ColleagueAction_ByLeader_ResendInvitation;
                //    break;
                case ColleagueAction.CancelInvitation:
                    templateId = EmailTemplateIds.ColleagueAction_ByLeader_CancelInvitation_ToMember;
                    break;
                default:
                    return;
            }

            await QueueColleagueActionEmailAsync(contentItem, colleague, templateId);
        }

        private async Task NotifyColleagueActionToNonMemberAsync(ContentItem contentItem, Colleague colleague, ColleagueAction colleagueAction)
        {
            if (colleagueAction != ColleagueAction.CancelInvitation)
            {
                return;
            }

            var part = contentItem.As<CenterProfilePart>();
            var leader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(part.MemberRightId);

            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                await scope.ServiceProvider.GetRequiredService<IEmailNotificationDataService>().QueueAsync(
                    new EmailNotification()
                    {
                        TemplateId = EmailTemplateIds.ColleagueAction_ByLeader_CancelInvitation_ToNonMember,
                        To = new HashSet<string>() { colleague.Email },
                        Data = new CenterProfileColleagueActionNotificationViewModel()
                        {
                            ColleagueName = colleague.FullName,
                            CenterName = part.CenterName,
                            LeaderName = leader.FullName
                        }
                    });
            }
        }

        private async Task QueueInvitationToMemberEmailAsync(ContentItem contentItem, Colleague colleague)
        {
            var part = contentItem.As<CenterProfilePart>();
            var leader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(part.MemberRightId);

            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                var nonceAccept = new Nonce()
                {
                    RedirectUrl = "~/" + CenterProfileNamedRoutes.CenterProfile_Colleagues_Apply + contentItem.ContentItemId,
                    Type = NonceType.MemberRightId,
                    TypeId = colleague.MemberRightId.Value
                };

                var nonceReject = new Nonce()
                {
                    RedirectUrl = "~/" + CenterProfileNamedRoutes.CenterProfile_Colleagues_Cancel + contentItem.ContentItemId,
                    Type = NonceType.MemberRightId,
                    TypeId = colleague.MemberRightId.Value
                };

                var nonceService = scope.ServiceProvider.GetRequiredService<INonceService>();
                await nonceService.CreateManyAsync(new[] { nonceAccept, nonceReject });

                await scope.ServiceProvider.GetRequiredService<IEmailNotificationDataService>().QueueAsync(
                    new EmailNotification()
                    {
                        TemplateId = EmailTemplateIds.ColleagueAction_ByLeader_ColleagueInvitation_ToMember,
                        To = new HashSet<string>() { colleague.Email },
                        Data = new ColleagueInvitationByLeaderToMemberViewModel()
                        {
                            ColleagueName = colleague.FullName,
                            CenterName = part.CenterName,
                            LeaderName = leader.FullName,
                            Occupation = colleague.Occupation,
                            NonceAccept = nonceAccept.Value,
                            NonceReject = nonceReject.Value
                        }
                    });
            }
        }

        private async Task QueueInvitationEmailToNonMemberAsync(ContentItem contentItem, Colleague colleague)
        {
            var part = contentItem.As<CenterProfilePart>();
            var leader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(part.MemberRightId);

            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                await scope.ServiceProvider.GetRequiredService<IEmailNotificationDataService>().QueueAsync(
                    new EmailNotification()
                    {
                        TemplateId = EmailTemplateIds.ColleagueAction_ByLeader_ColleagueInvitation_ToNonMember,
                        To = new HashSet<string>() { colleague.Email },
                        Data = new ColleagueInvitationByLeaderToNonMemberViewModel()
                        {
                            ColleagueName = colleague.FullName,
                            CenterName = part.CenterName,
                            LeaderName = leader.FullName,
                            Occupation = colleague.Occupation,
                            AfterSignUpUrl = $"/{NamedRoutes.DokiNetLoginPath}?returnUrl=/Colleagues/Apply/" + contentItem.ContentItemId
                        }
                    });
            }
        }

        private async Task QueueColleagueActionEmailAsync(ContentItem contentItem, Colleague colleague, string templateId)
        {
            var part = contentItem.As<CenterProfilePart>();
            var leader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(part.MemberRightId);

            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                var nonce = new Nonce()
                {
                    Type = NonceType.MemberRightId,
                    TypeId = colleague.MemberRightId.Value,
                    RedirectUrl = "~/" + CenterProfileNamedRoutes.CenterProfile_Colleagues
                };

                await scope.ServiceProvider.GetRequiredService<INonceService>().CreateAsync(nonce);

                await scope.ServiceProvider.GetRequiredService<IEmailNotificationDataService>().QueueAsync(
                    new EmailNotification()
                    {
                        TemplateId = templateId,
                        To = new HashSet<string>() { colleague.Email },
                        Data = new CenterProfileColleagueAsMemberNotificationByLeaderViewModel()
                        {
                            ColleagueName = colleague.FullName,
                            CenterName = part.CenterName,
                            LeaderName = leader.FullName,
                            Occupation = colleague.Occupation,
                            Nonce = nonce.Value
                        }
                    });
            }
        }
    }
}
