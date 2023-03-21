using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell;
using OrchardCore.Users;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Validators;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.DiabetesCareCenterManager.Extensions;
using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Login.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers.Api
{
    [Authorize]
    [Route("api/center-profile", Name = "CenterProfileApi")]
    public class CenterProfileApiController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICenterProfileCommonService _centerProfileCommonService;
        private readonly ICenterProfileService _centerProfileService;
        private readonly ICenterProfileValidator _centerProfileValidator;
        private readonly IDiabetesUserProfileService _diabetesUserProfileService;
        private readonly IDokiNetService _dokiNetService;
        private readonly IEmailNotificationDataService _emailNotificationDataService;
        private readonly IEmailSettingsService _emailSettingsService;
        private readonly ILogger _logger;
        private readonly ISharedUserService _sharedUserService;
        private readonly ITerritoryService _territoryService;
        private readonly ShellSettings _shellSettings;
        private readonly UserManager<IUser> _userManager;


        public IHtmlLocalizer T { get; set; }


        public CenterProfileApiController(
            IAuthorizationService authorizationService,
            ICenterProfileCommonService centerProfileCommonService,
            ICenterProfileService centerProfileService,
            ICenterProfileValidator centerProfileValidator,
            IDiabetesUserProfileService diabetesUserProfileService,
            IDokiNetService dokiNetService,
            IEmailNotificationDataService emailNotificationDataService,
            IEmailSettingsService emailSettingsService,
            IHtmlLocalizer<CenterProfileApiController> htmlLocalizer,
            ILogger<CenterProfileApiController> logger,
            ISharedUserService sharedUserService,
            ITerritoryService territoryService,
            ShellSettings shellSettings,
            UserManager<IUser> userManager)
        {
            _authorizationService = authorizationService;
            _centerProfileCommonService = centerProfileCommonService;
            _centerProfileService = centerProfileService;
            _centerProfileValidator = centerProfileValidator;
            _diabetesUserProfileService = diabetesUserProfileService;
            _dokiNetService = dokiNetService;
            _emailNotificationDataService = emailNotificationDataService;
            _emailSettingsService = emailSettingsService;
            _logger = logger;
            _sharedUserService = sharedUserService;
            _territoryService = territoryService;
            _shellSettings = shellSettings;
            _userManager = userManager;

            T = htmlLocalizer;
        }


        [HttpGet, Route("search-settlement", Name = "CenterProfileApi.SearchSettlement")]
        public async Task<IActionResult> Get(int zipCode, string settlement = null)
            => Ok(await _centerProfileCommonService.SearchTerritoryByZipCodeAsync(zipCode, settlement));

        [HttpGet]
        [Route("get-status", Name = "CenterProfileApi.GetAccreditationStatus")]
        public async Task<IActionResult> GetAccreditationStatus(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfile))
            {
                return Unauthorized();
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
                    return Unauthorized();
                }

                var contentItem = await _centerProfileService.GetCenterProfileToLeaderAsync(dokiNetMember.MemberRightId, id);
                if (contentItem == null)
                {
                    return NotFound();
                }

                if (contentItem.As<CenterProfilePart>().Created)
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
            catch (UserHasNoMemberRightIdException)
            {
                return Unauthorized();
            }
        }

        [HttpGet, Route("")]
        public async Task<IActionResult> Get(string id, [FromQuery(Name = "s")]CenterProfileEditorStep step)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfile))
            {
                return Unauthorized();
            }

            try
            {
                var isNew = string.IsNullOrEmpty(id);
                if (isNew && step != CenterProfileEditorStep.BasicData)
                {
                    return BadRequest();
                }

                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
                if (dokiNetMember == null || dokiNetMember.MemberRightId == 0)
                {
                    return Unauthorized();
                }

                ContentItem contentItem;
                if (isNew)
                {
                    contentItem = await _centerProfileService.NewCenterProfileAsync(dokiNetMember.MemberRightId, false);
                }
                else
                {
                    contentItem = await _centerProfileService.GetCenterProfileToLeaderAsync(dokiNetMember.MemberRightId, id);
                    if (contentItem == null)
                    {
                        return NotFound();
                    }

                    if (contentItem.As<CenterProfilePart>().Created)
                    {
                        return BadRequest();
                    }
                }

                ICenterProfileViewModel viewModel;
                object additionalData = null;
                switch (step)
                {
                    case CenterProfileEditorStep.BasicData:
                        viewModel = new CenterProfileBasicDataViewModel();
                        additionalData = CenterProfileHelpers.GetBasicAdditionalData(dokiNetMember);
                        break;
                    case CenterProfileEditorStep.AdditionalData:
                        viewModel = new CenterProfileAdditionalDataViewModel();
                        break;
                    case CenterProfileEditorStep.Equipments:
                        viewModel = new CenterProfileEquipmentsViewModel();
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
            catch (UserHasNoMemberRightIdException)
            {
                return Unauthorized();
            }
        }

        [HttpGet, Route("summary", Name = "CenterProfileApi.Summary")]
        public async Task<IActionResult> Get(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfile))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
            if (dokiNetMember == null || dokiNetMember.MemberRightId == 0)
            {
                return Unauthorized();
            }

            var contentItem = await _centerProfileService.GetCenterProfileToLeaderAsync(dokiNetMember.MemberRightId, id);
            if (contentItem == null)
            {
                return NotFound();
            }

            if (contentItem.As<CenterProfilePart>().Created)
            {
                return BadRequest();
            }

            return Ok(new
            {
                viewModel = CenterProfileComplexViewModel.CreateViewModel(contentItem, true, true, true, true, true, dokiNetMember)
            });
        }

        [HttpGet, Route("submit", Name = "CenterProfileApi.Submit")]
        public async Task<IActionResult> Submit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfile))
            {
                return Unauthorized();
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
                    return Unauthorized();
                }

                var contentItem = await _centerProfileService.GetCenterProfileToLeaderAsync(dokiNetMember.MemberRightId, id);
                if (contentItem == null)
                {
                    return NotFound();
                }

                var part = contentItem.As<CenterProfilePart>();
                if (part.Created)
                {
                    return BadRequest();
                }

                _centerProfileValidator.ValidateBasicData(part, T, ModelState.AddModelError);
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        step = CenterProfileEditorStep.BasicData.ToString(),
                        errors = ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage))
                    });
                }

                _centerProfileValidator.ValidateAdditionalData(part, T, ModelState.AddModelError);
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        step = CenterProfileEditorStep.AdditionalData.ToString(),
                        errors = ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage))
                    });
                }

                await _centerProfileService.SaveCenterProfileAsync(contentItem, true);

                await NotifyCreatedCenterProfileAsync(contentItem);

                return Ok();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById");

                return Conflict();
            }
        }

        [HttpPost, Route("basic-data", Name = "CenterProfileApi.BasicData")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post(string id, [FromBody]CenterProfileBasicDataViewModel viewModel)
            => await SubmitCenterProfileAsync(id, viewModel);

        [HttpPost, Route("additional-data", Name = "CenterProfileApi.AdditionalData")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post(string id, [FromBody]CenterProfileAdditionalDataViewModel viewModel)
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

            return await SubmitCenterProfileAsync(id, viewModel);
        }

        [HttpPost, Route("equipments", Name = "CenterProfileApi.Equipments")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post(string id, [FromBody]CenterProfileEquipmentsViewModel viewModel)
            => await SubmitCenterProfileAsync(id, viewModel);

        [HttpPost, Route("accept-many", Name = "CenterProfileApi.AcceptMany")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AcceptMany(CenterProfileMakeDecisionViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.MakingDecisionAboutCenterProfiles))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var idsOfAcceptedCenterProfiles = await _centerProfileService.AcceptManyAsync(viewModel.States);

                await NotifyAcceptedCenterProfileAsync(idsOfAcceptedCenterProfiles);

                return Ok();
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is ArgumentException)
            {
                return BadRequest();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById");

                return Conflict();
            }
        }

        [HttpGet, Route("view-colleague-profile", Name = "CenterProfileApi.ViewColleagueProfile")]
        public async Task<IActionResult> GetColleagueProfile(string id, Guid colleagueId)
        {
            var contentItem = await _centerProfileService.GetCenterProfileAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }

            var colleague = contentItem.As<CenterProfilePart>().Colleagues.FirstOrDefault(x => x.Id == colleagueId);
            if (colleague == null || !colleague.MemberRightId.HasValue)
            {
                return BadRequest();
            }

            return Ok(await _diabetesUserProfileService.GetPersonDataCompactViewModel(colleague.MemberRightId.Value));
        }

        [HttpGet, Route("view-leader-profile", Name = "CenterProfileApi.ViewLeaderProfile")]
        public async Task<IActionResult> GetLeaderProfile(int memberRightId)
            => Ok(await _diabetesUserProfileService.GetPersonDataCompactViewModel(memberRightId));


        private async Task<IActionResult> SubmitCenterProfileAsync(string id, ICenterProfileViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfile))
            {
                return Unauthorized();
            }

            var isNew = string.IsNullOrEmpty(id);
            if (isNew && !(viewModel is CenterProfileBasicDataViewModel))
            {
                ModelState.AddModelError("", "Operation not allowed");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();
                if (dokiNetMember == null || dokiNetMember.MemberRightId == 0)
                {
                    return Unauthorized();
                }

                var contentItem = isNew
                    ? await _centerProfileService.NewCenterProfileAsync(dokiNetMember.MemberRightId, true)
                    : await _centerProfileService.GetCenterProfileToLeaderAsync(dokiNetMember.MemberRightId, id);

                if (contentItem == null)
                {
                    return NotFound();
                }

                await _centerProfileService.SaveCenterProfileAsync(contentItem, viewModel);

                return Ok(new
                {
                    redirectUrl = Url.Action("Edit", "CenterProfile", new { id = contentItem.ContentItemId })
                });
            }
            catch (CenterProfileNotAssignedException)
            {
                return NotFound();
            }
        }

        private async Task NotifyCreatedCenterProfileAsync(ContentItem contentItem)
        {
            var part = contentItem.As<CenterProfilePart>();
            var leader = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(part.MemberRightId);

            var emailSettings = await _emailSettingsService.GetEmailSettingsAsync();

            await Task.WhenAll(
                _emailNotificationDataService.QueueAsync(new EmailNotification()
                {
                    To = new HashSet<string>() { leader.Emails.FirstOrDefault() },
                    TemplateId = EmailTemplateIds.CenterProfileCreatedFeedback,
                    Data = new CenterProfileCreatedTemplateViewModel()
                    {
                        CenterName = part.CenterName,
                        LeaderName = leader.FullName
                    }
                }),
                _emailNotificationDataService.QueueAsync(new EmailNotification()
                {
                    To = emailSettings.DebugEmailAddresses,
                    TemplateId = EmailTemplateIds.CenterProfileCreated,
                    Data = new CenterProfileCreatedTemplateViewModel()
                    {
                        CenterName = part.CenterName,
                        LeaderName = leader.FullName
                    }
                }));
        }

        private async Task NotifyAcceptedCenterProfileAsync(IEnumerable<string> idsOfAcceptedCenterProfiles)
        {
            var managerSettings = await _centerProfileCommonService.GetCenterManagerSettingsAsync();

            foreach (var contentItemId in idsOfAcceptedCenterProfiles)
            {
                var contentItem = await _centerProfileService.GetCenterProfileAsync(contentItemId);

                // TODO: ha link is kell majd az email-be, akkor a tenant nevét le kell cserélni
                // erről: _shellSettings.Name a megfelelőre
                await EmailNotificationExtensions.NotifyAcceptedCenterProfileAsync(
                    contentItem,
                    CenterPosts.MDTManagement,
                    _shellSettings.Name,
                    _dokiNetService,
                    _sharedUserService,
                    _territoryService,
                    _userManager,
                    _emailNotificationDataService.QueueAsync);
            }
        }
    }
}
