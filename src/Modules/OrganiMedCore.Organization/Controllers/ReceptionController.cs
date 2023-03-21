using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Services;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Services;
using OrganiMedCore.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Controllers
{
    [Authorize]
    public class ReceptionController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ISiteService _siteService;
        private readonly IEVisitPatientProfileService _eVisitPatientProfileService;
        private readonly IOrganizationService _organizationService;
        private readonly INotifier _notifier;
        private readonly ICheckInManager _checkInManager;
        private readonly IClock _clock;
        private readonly IOrganizationUserProfileService _organizationUserProfileService;


        public dynamic New { get; set; }
        public IHtmlLocalizer T { get; set; }


        public ReceptionController(
            IAuthorizationService authorizationService,
            ISharedDataAccessorService sharedDataAccessorService,
            ISiteService siteService,
            IEVisitPatientProfileService eVisitPatientProfileService,
            IShapeFactory shapeFactory,
            IOrganizationService organizationService,
            INotifier notifier,
            IHtmlLocalizer<ReceptionController> htmlLocalizer,
            ICheckInManager checkInManager,
            IClock clock,
            IOrganizationUserProfileService organizationUserProfileService)
        {
            _authorizationService = authorizationService;
            _sharedDataAccessorService = sharedDataAccessorService;
            _siteService = siteService;
            _eVisitPatientProfileService = eVisitPatientProfileService;
            _organizationService = organizationService;
            _notifier = notifier;
            _checkInManager = checkInManager;
            _clock = clock;
            _organizationUserProfileService = organizationUserProfileService;

            New = shapeFactory;
            T = htmlLocalizer;
        }


        // The daily list which contains all organization units.
        [Route("recepcio")]
        public async Task<IActionResult> Index(DateTime date)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageReception))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                date = date == default(DateTime) ? _clock.UtcNow : date.ToUniversalTime();
                var viewModel = await _checkInManager.GetCheckedInPatientsAsync(scope, date);

                ViewData["Date"] = date.ToLocalTime().Date;

                return View(viewModel);
            }
        }

        // The list of the search. The search will happen among the shared profiles and could narrowed down by the organization tenant's patients.
        [Route("recepcio/kereses")]
        public async Task<IActionResult> Search(EVisitPatientProfileFilter filter)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageReception))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var contentItems = Enumerable.Empty<ContentItem>();

                if (filter != null && !filter.IsEmpty())
                {
                    // Getting max 101 results so in the UI we can display a message if there's more than 100 results.
                    // So the user will know that he must define a more specific search.
                    var result = await _eVisitPatientProfileService.SearchAsync(scope, filter, 101);
                    if (result != null)
                    {
                        contentItems = result;
                    }
                }

                var viewModel = (await New.ViewModel())
                            .ContentItems(contentItems)
                            .Filter(filter);

                return View(viewModel);
            }
        }

        [Route("recepcio/felvetel-napi-listara/{id}")]
        public async Task<IActionResult> AddToDailyList(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageReception))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var patient = await _eVisitPatientProfileService.GetAsync(scope, id);
                if (patient == null)
                {
                    NotFound();
                }

                var viewModel = new AddToDailyListViewModel
                {
                    EVisitPatientProfileId = id
                };

                SetViewData(patient.As<EVisitPatientProfilePart>().FullName, await _organizationService.ListOrganizationUnitsAsync());
                return View(viewModel);
            }
        }

        [HttpPost, ActionName("AddToDailyList")]
        [Route("recepcio/felvetel-napi-listara/{id}")]
        public async Task<IActionResult> AddToDailyListPost(AddToDailyListViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageReception))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var patient = await _eVisitPatientProfileService.GetAsync(scope, viewModel.EVisitPatientProfileId);
                if (patient == null)
                {
                    NotFound();
                }

                var organizationsUnits = await _organizationService.ListOrganizationUnitsAsync();
                if (!organizationsUnits.Any(x => x.ContentItem.ContentItem.ContentItemId == viewModel.OrganizationUnitId))
                {
                    ModelState.AddModelError(nameof(AddToDailyListViewModel.OrganizationUnitId), "Osztály választása kötelező.");
                }

                var doctors = await _organizationUserProfileService.GetDoctors(scope, viewModel.OrganizationUnitId);
                if (!doctors.Any(x => x.EVisitOrganizationUserProfilePart.ContentItem.ContentItemId == viewModel.EVisitOrganizationUserProfileId))
                {
                    ModelState.AddModelError(nameof(AddToDailyListViewModel.EVisitOrganizationUserProfileId), "Doktor választása kötelező.");
                }

                var patientName = patient.As<EVisitPatientProfilePart>().FullName;
                if (!ModelState.IsValid)
                {
                    SetViewData(patientName, organizationsUnits);
                    return View(viewModel);
                }

                // Adding to daily list.
                try
                {
                    await _checkInManager.CheckInPatient(scope, viewModel.EVisitPatientProfileId, viewModel.OrganizationUnitId, viewModel.EVisitOrganizationUserProfileId);
                }
                catch (Exception ex)
                {
                    if (ex.IsFatal())
                    {
                        throw;
                    }

                    _notifier.Error(T["Hiba a páciens napi listára való felvétele során: {0}", ex.Message]);
                    SetViewData(patientName, organizationsUnits);
                    return View(viewModel);
                }

                _notifier.Success(T["{0} páciens sikeresen hozzáadva a napi listához.", patientName]);
                return RedirectToAction("Index");
            }
        }

        private void SetViewData(string patientName, IEnumerable<OrganizationUnitPart> organizationUnits)
        {
            ViewData["OrganizationUnits"] = organizationUnits;
            ViewData["PatientName"] = patientName;
        }
    }
}
