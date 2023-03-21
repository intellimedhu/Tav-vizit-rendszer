using IntelliMed.Core;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Models.Enums;
using OrganiMedCore.Core.Services;
using OrganiMedCore.Core.ViewModels;
using OrganiMedCore.Login.Services;
using OrganiMedCore.Organization.ActionFilters;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Services;
using OrganiMedCore.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.Organization.Controllers
{
    [Authorize]
    public class PatientsController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IEVisitPatientProfileService _eVisitPatientProfileService;
        private readonly IAccessLogService _accessLogService;
        private readonly IBetterUserService _betterUserService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISiteService _siteService;
        private readonly ISharedLoginService _sharedLoginService;
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly ICheckInManager _checkInManager;
        private readonly IClock _clock;
        private readonly IOrganizationService _organizationService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;


        public IHtmlLocalizer T { get; set; }
        public IStringLocalizer S { get; set; }
        public dynamic New { get; set; }


        public PatientsController(
            IAuthorizationService authorizationService,
            IHtmlLocalizer<PatientsController> htmlLocalizer,
            IStringLocalizer<PatientsController> stringLocalizer,
            INotifier notifier,
            IEVisitPatientProfileService eVisitPatientProfileService,
            IAccessLogService accessLogService,
            IBetterUserService betterUserService,
            IShapeFactory shapeFactory,
            ISharedDataAccessorService sharedDataAccessorService,
            ISiteService siteService,
            ISharedLoginService sharedLoginService,
            ISession session,
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            ICheckInManager checkInManager,
            IClock clock,
            IOrganizationService organizationService,
            IPasswordGeneratorService passwordGeneratorService)
        {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _eVisitPatientProfileService = eVisitPatientProfileService;
            _accessLogService = accessLogService;
            _betterUserService = betterUserService;
            _sharedDataAccessorService = sharedDataAccessorService;
            _siteService = siteService;
            _sharedLoginService = sharedLoginService;
            _session = session;
            _contentManager = contentManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _checkInManager = checkInManager;
            _clock = clock;
            _organizationService = organizationService;
            _passwordGeneratorService = passwordGeneratorService;

            T = htmlLocalizer;
            S = stringLocalizer;
            New = shapeFactory;
        }


        // The daily list of the signed in organization unit.
        [ServiceFilter(typeof(OrganizationUnitActionFilter))]
        [Route("paciensek")]
        public async Task<IActionResult> Index([FromQuery(Name = "date")]DateTime? checkInDate)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePatinets))
            {
                return Unauthorized();
            }

            checkInDate = checkInDate == default(DateTime?) ? _clock.UtcNow : checkInDate.Value.ToUniversalTime();

            DailyListViewModel viewModel;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var organizationUnit = await _organizationService.GetSignedInOrganizationUnitAsync(scope, await _betterUserService.GetCurrentUserAsync());
                viewModel = await _checkInManager.GetCheckedInPatientsAsync(scope, checkInDate.Value, organizationUnit.ContentItem.ContentItemId);
            }

            ViewData["Date"] = checkInDate;

            return View(viewModel);
        }

        [HttpGet]
        [Route("paciensek/azonosito-ellenorzes")]
        public async Task<IActionResult> CheckId()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePatinets))
            {
                return Unauthorized();
            }

            return View(new PatientIdentifierViewModel() { Type = PatientIdentifierTypes.Taj });
        }

        [HttpPost, ActionName("CheckId")]
        [Route("paciensek/azonosito-ellenorzes")]
        public async Task<IActionResult> CheckIdPost(PatientIdentifierViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePatinets))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (viewModel.Type == PatientIdentifierTypes.None)
            {
                return RedirectToAction(nameof(Edit));
            }

            ContentItem existingPatient;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                existingPatient = await _eVisitPatientProfileService.GetByIdentifierAsync(scope, viewModel.Value, viewModel.Type);
            }

            if (existingPatient != null)
            {
                _notifier.Information(T["A megadott azonosítójú páciens már létezik. Itt tudja szerkeszteni."]);

                return RedirectToAction(nameof(Edit), new { eVisitPatientProfileId = existingPatient.ContentItemId });
            }

            _notifier.Information(T["A megadott azonosítóval még nem létezik páciens. Itt tudja létrehozni."]);

            return RedirectToAction(nameof(Edit), viewModel);
        }

        [HttpGet]
        [Route("paciensek/szerkesztes/{eVisitPatientProfileId?}")]
        public async Task<IActionResult> Edit(string eVisitPatientProfileId, [FromQuery]PatientIdentifierViewModel patientIdentifierViewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePatinets))
            {
                return Unauthorized();
            }

            var isNew = string.IsNullOrEmpty(eVisitPatientProfileId);

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var patient = isNew
                    ? await _eVisitPatientProfileService.InitializeAsync(scope)
                    : await _eVisitPatientProfileService.GetAsync(scope, eVisitPatientProfileId);
                if (patient == null)
                {
                    return NotFound();
                }

                if (isNew && patientIdentifierViewModel != null)
                {
                    patient.Alter<EVisitPatientProfilePart>(part =>
                    {
                        part.PatientIdentifierType = patientIdentifierViewModel.Type;
                        part.PatientIdentifierValue = patientIdentifierViewModel.Value;
                    });
                }
                else
                {
                    var patientProfile = await _session
                        .Query<ContentItem, PatientProfilePartIndex>(x => x.EVisitPatientProfileId == patient.ContentItemId)
                        .LatestAndPublished()
                        .FirstOrDefaultAsync();
                }

                ViewData["IsNew"] = isNew;
                ViewData["EVisitPatientProfileId"] = eVisitPatientProfileId;

                var shape = await _sharedDataAccessorService.BuildManagerEditorAsync(scope, patient, this, isNew);

                return View(shape);
            }
        }

        [HttpPost, ActionName("Edit")]
        [Route("paciensek/szerkesztes/{eVisitPatientProfileId?}")]
        public async Task<IActionResult> EditPost(string eVisitPatientProfileId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePatinets))
            {
                return Unauthorized();
            }

            var isNew = string.IsNullOrEmpty(eVisitPatientProfileId);

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var patient = isNew
                    ? await _eVisitPatientProfileService.InitializeAsync(scope)
                    : await _eVisitPatientProfileService.GetNewVersionAsync(scope, eVisitPatientProfileId);
                if (patient == null)
                {
                    return NotFound();
                }

                var previousEmail = patient.As<EVisitPatientProfilePart>().Email;

                var managerContentItemDisplayManager = scope.ServiceProvider.GetRequiredService<IContentItemDisplayManager>();
                var managerSession = scope.ServiceProvider.GetRequiredService<ISession>();

                var model = await managerContentItemDisplayManager.UpdateEditorAsync(patient, this, false);
                var eVisitPatientPart = patient.As<EVisitPatientProfilePart>();

                if (!ModelState.IsValid)
                {
                    managerSession.Cancel();
                    ViewData["IsNew"] = isNew;

                    return View(nameof(Edit), model);
                }

                // If there's no user attached then check the availability of the e-mail.
                // TODO: check in the users and in the patientprofiles too.
                if (eVisitPatientPart.SharedUserId == 0
                    && !string.IsNullOrEmpty(eVisitPatientPart.Email))
                {
                    var managerUserManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                    if (await managerUserManager.FindByEmailAsync(eVisitPatientPart.Email) != null)
                    {
                        ModelState.AddModelError(nameof(eVisitPatientPart.Email), S["Már létezik felhasználó vagy páciens ilyen e-mail címmel, ezért nem adhatja meg ezt az e-mail címet."]);
                    }
                    else
                    {
                        var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
                        var user = await _sharedLoginService.SharedRegisterAsync(
                            scope,
                            eVisitPatientPart.Email,
                            !settings.UsersMustValidateEmail,
                            _passwordGeneratorService.GenerateRandomPassword(16),
                            new List<string>() { OrganizationRoleNames.Patient },
                            null,
                            (key, message) => ModelState.AddModelError(key, message));

                        _notifier.Success(T["OrganiMed felhasználó sikeresen létrehozva ehhez a pácienshez."]);

                        patient.Alter<EVisitPatientProfilePart>(x => x.SharedUserId = ((User)user).Id);
                    }

                    if (!ModelState.IsValid)
                    {
                        managerSession.Cancel();
                        ViewData["IsNew"] = isNew;

                        return View(nameof(Edit), model);
                    }
                }

                // TODO: check confirmation emails. While it isn't confirmed the e-mail can be changed.
                // Meanwhile the above TODO exists the e-mail just can't be changed.
                if (!string.IsNullOrEmpty(previousEmail))
                {
                    patient.Alter<EVisitPatientProfilePart>(x => x.Email = previousEmail);
                }

                // Clearing previously saved value.
                if (eVisitPatientPart.PatientIdentifierType == PatientIdentifierTypes.None)
                {
                    patient.Alter<EVisitPatientProfilePart>(x => x.PatientIdentifierValue = string.Empty);
                }

                if (isNew)
                {
                    await scope.ServiceProvider.GetRequiredService<IContentManager>().CreateAsync(patient);
                }
                else
                {
                    await scope.ServiceProvider.GetRequiredService<IContentManager>().PublishAsync(patient);
                }

                _notifier.Success(T["A páciens mentése sikeres."]);

                await _accessLogService.LogContentActivityAsync(scope, await _betterUserService.ConvertUserAsync(User), Crud.Update, patient);

                return RedirectToAction(nameof(Edit), new { eVisitPatientProfileId = patient.ContentItemId });
            }
        }

        [HttpGet]
        [Route("paciensek/szerkesztes-kiegeszito/{eVisitPatientProfileId}")]
        public async Task<IActionResult> EditAdditional(string eVisitPatientProfileId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePatinets))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(eVisitPatientProfileId))
            {
                return BadRequest();
            }

            ContentItem eVisitPatientProfile;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                eVisitPatientProfile = await _eVisitPatientProfileService.GetNewVersionAsync(scope, eVisitPatientProfileId);
                if (eVisitPatientProfile == null)
                {
                    return NotFound();
                }
            }

            var localPatientProfile = await _session
                .Query<ContentItem, PatientProfilePartIndex>(index => index.EVisitPatientProfileId == eVisitPatientProfileId)
                .FirstOrDefaultAsync();

            var isNew = localPatientProfile == null;
            if (isNew)
            {
                localPatientProfile = await _contentManager.NewAsync(ContentTypes.PatientProfile);
            }

            ViewData["EVisitPatientProfileId"] = eVisitPatientProfileId;

            var shape = await _contentItemDisplayManager.BuildEditorAsync(localPatientProfile, this, isNew);

            return View(shape);
        }

        [HttpPost, ActionName("EditAdditional")]
        [Route("paciensek/szerkesztes-kiegeszito/{eVisitPatientProfileId?}")]
        public async Task<IActionResult> EditAdditionalPost(string eVisitPatientProfileId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePatinets))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(eVisitPatientProfileId))
            {
                return BadRequest();
            }

            ContentItem eVisitPatientProfile;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                eVisitPatientProfile = await _eVisitPatientProfileService.GetAsync(scope, eVisitPatientProfileId);
                if (eVisitPatientProfile == null)
                {
                    return NotFound();
                }
            }

            var localPatientProfile = await _session
                .Query<ContentItem, PatientProfilePartIndex>(index => index.EVisitPatientProfileId == eVisitPatientProfileId)
                .FirstOrDefaultAsync();
            var isNew = localPatientProfile == null;

            var model = await _contentItemDisplayManager.UpdateEditorAsync(localPatientProfile, this, isNew);
            if (!ModelState.IsValid)
            {
                _session.Cancel();

                ViewData["EVisitPatientProfileId"] = eVisitPatientProfileId;

                return View("EditAdditional", model);
            }

            // Set the link between the two patient profiles.
            if (string.IsNullOrEmpty(localPatientProfile.As<PatientProfilePart>().EVisitPatientProfileId))
            {
                localPatientProfile.Alter<PatientProfilePart>(x => x.EVisitPatientProfileId = eVisitPatientProfileId);
            }

            _notifier.Success(T["Patient saved."]);

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                await _accessLogService.LogContentActivityAsync(
                    scope,
                    await _betterUserService.ConvertUserAsync(User),
                    Crud.Update,
                    eVisitPatientProfile);
            }

            return RedirectToAction(nameof(EditAdditional), new { eVisitPatientProfileId });
        }
    }
}
