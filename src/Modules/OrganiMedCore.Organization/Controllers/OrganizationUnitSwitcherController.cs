using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Extensions;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Services;
using OrganiMedCore.Organization.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Controllers
{
    [Authorize]
    public class OrganizationUnitSwitcherController : Controller
    {
        private readonly IBetterUserService _betterUserService;
        private readonly INotifier _notifier;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationUserProfileService _organizationUserProfileService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public IHtmlLocalizer T { get; set; }


        public OrganizationUnitSwitcherController(
            IBetterUserService betterUserService,
            IHtmlLocalizer<OrganizationUnitSwitcherController> htmlLocalizer,
            INotifier notifier,
            IOrganizationUserProfileService organizationUserProfileService,
            IOrganizationService organizationService,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _betterUserService = betterUserService;
            _notifier = notifier;
            _organizationService = organizationService;
            _organizationUserProfileService = organizationUserProfileService;
            _sharedDataAccessorService = sharedDataAccessorService;

            T = htmlLocalizer;
        }


        [Route("osztalyos-bejelentkezes")]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole(OrganizationRoleNames.Doctor))
            {
                return Unauthorized();
            }

            ContentItem profile;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                profile = await _organizationUserProfileService.GetOrganizationUserProfileAsync(
                    scope,
                    await _betterUserService.GetCurrentUserAsync());
            }

            if (profile == null)
            {
                return NotFound();
            }

            var viewModel = new OrganizationUnitSwitcherViewModel()
            {
                SelectedOrganizationId = profile.As<OrganizationUserProfilePart>().SignedInOrganizationUnitId
            };

            ViewData["OrganizationUnits"] = await _organizationService.ListPermittedOrganizationUnitsAsync(profile.As<OrganizationUserProfilePart>());

            return View(viewModel);
        }

        [HttpPost, ActionName("Index")]
        [Route("osztalyos-bejelentkezes")]
        public async Task<IActionResult> IndexPost(OrganizationUnitSwitcherViewModel viewModel)
        {
            if (!User.IsInRole(OrganizationRoleNames.Doctor))
            {
                return Unauthorized();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _organizationService.SignInToOrganizationAsync(viewModel.SelectedOrganizationId, await _betterUserService.GetCurrentUserAsync());

                    _notifier.Success(T["Sikeresen bejelentkezett az osztályba."]);

                    return RedirectToAction("Index");
                }

                return View(viewModel);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (NotPermittedOrganizationUnitException)
            {
                _notifier.Error(T["Ki kell választani egy osztályt."]);

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
