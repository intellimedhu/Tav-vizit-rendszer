using IntelliMed.Core.Constants;
using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.Core.Exceptions;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Models.Enums;
using OrganiMedCore.Core.Services;
using OrganiMedCore.Login.Services;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Services;
using OrganiMedCore.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.Organization.Controllers
{
    [Authorize]
    public class OrganizationUserProfilesController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;
        private readonly INotifier _notifier;
        private readonly IEVisitOrganizationUserProfileService _eVisitOrganizationUserProfileService;
        private readonly IOrganizationUserProfileService _organizationUserProfileService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ShellSettings _shellSettings;
        private readonly ISharedLoginService _sharedLoginService;
        private readonly ISiteService _siteService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;


        public IStringLocalizer S { get; set; }
        public IHtmlLocalizer T { get; set; }


        public OrganizationUserProfilesController(
            IAuthorizationService authorizationService,
            ISession session,
            IHtmlLocalizer<OrganizationUserProfilesController> htmlLocalizer,
            INotifier notifier,
            IEVisitOrganizationUserProfileService eVisitOrganizationUserProfileService,
            IOrganizationUserProfileService organizationUserProfileService,
            ISharedDataAccessorService sharedDataAccessorService,
            ShellSettings shellSettings,
            IStringLocalizer<OrganizationUserProfilesController> stringLocalizer,
            ISharedLoginService sharedLoginService,
            ISiteService siteService,
            IContentItemDisplayManager contentItemDisplayManager)
        {
            _authorizationService = authorizationService;
            _session = session;
            _notifier = notifier;
            _eVisitOrganizationUserProfileService = eVisitOrganizationUserProfileService;
            _organizationUserProfileService = organizationUserProfileService;
            _sharedDataAccessorService = sharedDataAccessorService;
            _shellSettings = shellSettings;
            _sharedLoginService = sharedLoginService;
            _siteService = siteService;
            _contentItemDisplayManager = contentItemDisplayManager;

            S = stringLocalizer;
            T = htmlLocalizer;
        }


        [Route("intezmenyi-felhasznalok")]
        public async Task<ActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var profiles = (await _eVisitOrganizationUserProfileService.ListAsync(scope))
                    .OrderBy(x => x.As<EVisitOrganizationUserProfilePart>().Name);

                return View(profiles);
            }
        }

        [Route("intezmenyi-felhasznalok/letrehozas")]
        public async Task<IActionResult> Create(
            string identifier = "",
            OrganizationUserIdentifierTypes identifierType = OrganizationUserIdentifierTypes.StampNumber,
            OrganizationUserProfileTypes organizationUserProfileType = OrganizationUserProfileTypes.Doctor)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var profile = await _eVisitOrganizationUserProfileService.InitializeAsync(scope);
                if (!string.IsNullOrEmpty(identifier))
                {
                    switch (identifierType)
                    {
                        case OrganizationUserIdentifierTypes.Email:
                            profile.Alter<EVisitOrganizationUserProfilePart>(x =>
                            {
                                x.Email = identifier;
                            });
                            break;
                        case OrganizationUserIdentifierTypes.StampNumber:
                            profile.Alter<EVisitOrganizationUserProfilePart>(x =>
                            {
                                x.StampNumber = identifier;
                            });
                            break;
                        default:
                            break;
                    }
                }

                profile.Alter<EVisitOrganizationUserProfilePart>(x =>
                {
                    x.OrganizationUserProfileType = organizationUserProfileType;
                });

                var shape = await _sharedDataAccessorService.BuildManagerEditorAsync(scope, profile, this, true);

                return View(shape);
            }
        }

        [HttpPost]
        [ActionName(nameof(Create))]
        [Route("intezmenyi-felhasznalok/letrehozas")]
        public async Task<IActionResult> CreatePost()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var profile = await _eVisitOrganizationUserProfileService.InitializeAsync(scope);
                var shape = await _sharedDataAccessorService.UpdateManagerEditorAsync(scope, profile, this, true);

                var profilePart = profile.As<EVisitOrganizationUserProfilePart>();

                if ((await _eVisitOrganizationUserProfileService.GetByIdentifierAsync(scope, profilePart.StampNumber, OrganizationUserIdentifierTypes.StampNumber)) != null)
                {
                    ModelState.AddModelError("EVisitOrganizationUserProfile.StampNumber", S["Ilyen pecsétszámmal már létezik intézményi felhasználó a rendszerben."]);
                }

                if (!ModelState.IsValid)
                {
                    return View(shape);
                }

                try
                {
                    // Creating EVisit user.
                    var managerUserManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                    var managerPasswordGeneratorService = scope.ServiceProvider.GetRequiredService<IPasswordGeneratorService>();
                    if (await managerUserManager.FindByEmailAsync(profilePart.Email) != null)
                    {
                        ModelState.AddModelError("EVisitOrganizationUserProfile.Email", S["Már létezik felhasználó ezzel az e-mail címmel, ezért nem adhatja meg ezt az e-mail címet."]);
                    }
                    else
                    {
                        var settings = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>();
                        var localUser = (User)await _sharedLoginService.SharedRegisterAsync(
                            scope,
                            profilePart.Email,
                            !settings.UsersMustValidateEmail,
                            managerPasswordGeneratorService.GenerateRandomPassword(16),
                            // TODO: add roles
                            new List<string>(),
                            null,
                            (key, message) => ModelState.AddModelError(key, message));

                        if (localUser == null)
                        {
                            ModelState.AddModelError("Error", S["Hiba a lokális felhasználó létrehozása közben."]);
                        }

                        if (!int.TryParse(localUser.UserName, out int sharedUserId))
                        {
                            ModelState.AddModelError("Error", S["OrganiMed felhasználó hibás username."]);
                        }

                        profile.Alter<EVisitOrganizationUserProfilePart>(x => x.SharedUserId = sharedUserId);

                        // Saving EVisit profile.
                        await scope.ServiceProvider.GetRequiredService<IContentManager>()
                            .CreateAsync(profile);

                        // Assigning EVisit user to the organization.
                        await _eVisitOrganizationUserProfileService.AssignToTenantAsync(
                            scope,
                            sharedUserId,
                            _shellSettings.Name,
                            // TODO: add roles.
                            new List<string>(),
                            true,
                            (key, message) => ModelState.AddModelError(key, message));
                    }

                    if (!ModelState.IsValid)
                    {
                        var managerSession = scope.ServiceProvider.GetRequiredService<ISession>();
                        managerSession.Cancel();
                        _session.Cancel();

                        return View(shape);
                    }

                    _notifier.Success(T["OrganiMed felhasználó létrehozása sikeres."]);
                    return RedirectToAction(nameof(Edit), new { id = profile.ContentItemId });
                }
                catch (Exception ex) when (ex is UserNotFoundException || ex is EVisitProfileNotFoundException || ex is OrganizationNotFoundException)
                {
                    ModelState.AddModelError("Error", S[ex.Message]);

                    var managerSession = scope.ServiceProvider.GetRequiredService<ISession>();
                    managerSession.Cancel();
                    _session.Cancel();

                    return View(shape);
                }
            }
        }

        [Route("intezmenyi-felhasznalok/szerkesztes/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var eVisitProfile = await _eVisitOrganizationUserProfileService.GetAsync(scope, id);
                if (eVisitProfile == null)
                {
                    return NotFound();
                }

                var profile = await _session
                    .Query<ContentItem, OrganizationUserProfilePartIndex>(x => x.EVisitOrganizationUserProfileId == id)
                    .FirstOrDefaultAsync();

                ViewData["ProfileId"] = profile == null ? string.Empty : profile.ContentItemId;
                ViewData["EVisitProfileId"] = id;

                var shape = await _sharedDataAccessorService.BuildManagerEditorAsync(scope, eVisitProfile, this, false);

                return View(shape);
            }
        }

        [HttpPost]
        [ActionName(nameof(Edit))]
        [Route("intezmenyi-felhasznalok/szerkesztes/{id}")]
        public async Task<IActionResult> EditPost(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var eVisitProfile = await _eVisitOrganizationUserProfileService.GetNewVerisonAsync(scope, id);
                if (eVisitProfile == null)
                {
                    return NotFound();
                }

                var previousEmail = eVisitProfile.As<EVisitOrganizationUserProfilePart>().Email;
                var shape = await _sharedDataAccessorService.UpdateManagerEditorAsync(scope, eVisitProfile, this, false);

                var eVisitProfilePart = eVisitProfile.As<EVisitOrganizationUserProfilePart>();

                var profileWithThisStampNumber = await _eVisitOrganizationUserProfileService.GetByIdentifierAsync(scope, eVisitProfilePart.StampNumber, OrganizationUserIdentifierTypes.StampNumber);
                if (profileWithThisStampNumber != null && eVisitProfile.ContentItemId != profileWithThisStampNumber.ContentItemId)
                {
                    ModelState.AddModelError("EVisitOrganizationUserProfile.StampNumber", S["Ilyen pecsétszámmal már létezik intézményi felhasználó a rendszerben."]);
                }

                if (!ModelState.IsValid)
                {
                    var profile = await _session
                    .Query<OrganizationUserProfilePart, OrganizationUserProfilePartIndex>(x => x.EVisitOrganizationUserProfileId == id)
                    .FirstOrDefaultAsync();

                    ViewData["ProfileId"] = profile == null ? string.Empty : profile.ContentItem.ContentItemId;
                    ViewData["EVisitProfileId"] = id;

                    return View(shape);
                }

                // TODO: check confirmation emails. While it isn't confirmed the e-mail can be changed. Also implement the email unique validation logic.
                // Meanwhile the above TODO exists the e-mail just can't be changed.
                if (!string.IsNullOrEmpty(previousEmail))
                {
                    eVisitProfile.Alter<EVisitOrganizationUserProfilePart>(x => x.Email = previousEmail);
                }

                // Save
                await scope.ServiceProvider.GetRequiredService<IContentManager>()
                    .PublishAsync(eVisitProfile);

                _notifier.Success(T["Intézményi felhasználó mentése sikeres."]);

                return RedirectToAction(nameof(Edit), new { id = eVisitProfile.ContentItemId });
            }
        }

        [HttpGet]
        [Route("intezmenyi-felhasznalok/szerkesztes-kiegeszito/{eVisitProfileId}/{id?}")]
        public async Task<IActionResult> EditAdditional(string eVisitProfileId, string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            var isNew = string.IsNullOrEmpty(id);

            try
            {
                var eVisitProfile = await GetEVisitProfileAsync(eVisitProfileId);
                var organizationProfile = await _organizationUserProfileService.GetOrganizationUserProfileAsync(
                    id,
                    eVisitProfile.ContentItemId,
                    eVisitProfile.As<EVisitOrganizationUserProfilePart>().OrganizationUserProfileType,
                    true);

                ViewData["EVisitProfileId"] = eVisitProfileId;
                ViewData["ProfileId"] = id;

                var shape = await _contentItemDisplayManager.BuildEditorAsync(organizationProfile, this, isNew);

                return View(shape);
            }
            catch (NotFoundException ex)
            {
                return NotFound(T[ex.Message].Value);
            }
        }

        [HttpPost, ActionName(nameof(EditAdditional))]
        [Route("intezmenyi-felhasznalok/szerkesztes-kiegeszito/{eVisitProfileId}/{id?}")]
        public async Task<IActionResult> EditAdditionalPost(string eVisitProfileId, string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            var isNew = string.IsNullOrEmpty(id);

            try
            {
                var eVisitProfile = await GetEVisitProfileAsync(eVisitProfileId);
                var organizationProfile = await _organizationUserProfileService.GetOrganizationUserProfileAsync(
                    id,
                    eVisitProfile.ContentItemId,
                    eVisitProfile.As<EVisitOrganizationUserProfilePart>().OrganizationUserProfileType,
                    false);

                var shape = await _contentItemDisplayManager.UpdateEditorAsync(organizationProfile, this, isNew);
                if (!ModelState.IsValid)
                {
                    _session.Cancel();

                    ViewData["EVisitProfileId"] = eVisitProfileId;
                    ViewData["ProfileId"] = id;

                    return View(shape);
                }

                await _organizationUserProfileService.SaveOrganizationUserProfileAsync(organizationProfile, isNew);

                _notifier.Success(T["Intézményi felhasználó mentése sikeres."]);

                return RedirectToAction(nameof(EditAdditional), new { eVisitProfileId, id = organizationProfile.ContentItemId });

            }
            catch (NotFoundException ex)
            {
                return NotFound(T[ex.Message].Value);
            }
        }

        [HttpGet]
        [Route("intezmenyi-felhasznalok/azonosito-ellenorzes")]
        public async Task<IActionResult> CheckExistingProfile()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            return View(new OrganizationUserIdentifierViewModel());
        }

        [HttpPost, ActionName("CheckExistingProfile")]
        [Route("intezmenyi-felhasznalok/azonosito-ellenorzes")]
        public async Task<IActionResult> CheckExistingProfilePost(OrganizationUserIdentifierViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            switch (viewModel.IdentifierType)
            {
                case OrganizationUserIdentifierTypes.Email:
                    if (!Regex.IsMatch(viewModel.Identifier, RegexPatterns.Email))
                    {
                        ModelState.AddModelError("OrganizationUserIdentifierViewModel.IdentifierType", S["Az e-mail formátuma nem megfelelő."]);
                    }
                    break;
                case OrganizationUserIdentifierTypes.StampNumber:
                    if (!Regex.IsMatch(viewModel.Identifier, RegexPatterns.DoctorStampNumber))
                    {
                        ModelState.AddModelError("OrganizationUserIdentifierViewModel.IdentifierType", S["A pecsétszám formátuma nem megfelelő."]);
                    }
                    break;
                default:
                    ModelState.AddModelError("OrganizationUserIdentifierViewModel.IdentifierType", S["Nem létező azonosító típus."]);
                    break;
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var profile = await _eVisitOrganizationUserProfileService.GetByIdentifierAsync(scope, viewModel.Identifier, viewModel.IdentifierType);
                viewModel.IdentifierChecked = true;
                if (profile != null)
                {
                    viewModel.OrganizationUserExist = true;
                }

                return View(viewModel);
            }
        }

        [HttpPost, ActionName("AddToOrganization")]
        public async Task<IActionResult> AddToOrganizationPost(string identifier, OrganizationUserIdentifierTypes identifierType)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var profile = await _eVisitOrganizationUserProfileService.GetByIdentifierAsync(scope, identifier, identifierType);
                if (profile == null)
                {
                    return NotFound();
                }

                await _eVisitOrganizationUserProfileService.AssignToTenantAsync(
                    scope,
                    profile.As<EVisitOrganizationUserProfilePart>().SharedUserId,
                    _shellSettings.Name,
                    new List<string>(),
                    true,
                    (key, message) => ModelState.AddModelError(key, message));

                if (!ModelState.IsValid)
                {
                    return RedirectToAction(nameof(CheckExistingProfile));
                }

                _notifier.Success(T["Intézményi felhasználó sikeresen hozzáadva az intézményhez. Itt tudja szerkeszteni."]);
                return RedirectToAction(nameof(Edit), new { id = profile.ContentItemId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOrganizationUserProfiles))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var profile = await _eVisitOrganizationUserProfileService.GetAsync(scope, id);
                if (profile == null)
                {
                    return NotFound();
                }

                await _eVisitOrganizationUserProfileService.RemoveFromOrganizationAsync(
                    scope,
                    profile.As<EVisitOrganizationUserProfilePart>().SharedUserId,
                    _shellSettings.Name,
                    (key, message) => ModelState.AddModelError(key, message));

                if (!ModelState.IsValid)
                {
                    var managerSession = scope.ServiceProvider.GetRequiredService<ISession>();
                    managerSession.Cancel();
                    _session.Cancel();

                    return RedirectToAction(nameof(Index));
                }

                _notifier.Success(T["Intézményi felhasználó sikeresen eltávolítva az intézménytől."]);
                return RedirectToAction(nameof(Index));
            }
        }


        private async Task<ContentItem> GetEVisitProfileAsync(string eVisitProfileId)
        {
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var profile = await _eVisitOrganizationUserProfileService.GetAsync(scope, eVisitProfileId);

                return profile ?? throw new NotFoundException("A kért profil nem található");
            }
        }
    }
}
