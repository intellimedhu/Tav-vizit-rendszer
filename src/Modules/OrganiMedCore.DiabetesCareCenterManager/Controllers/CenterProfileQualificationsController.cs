using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Admin]
    [Authorize]
    public class CenterProfileQualificationsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IQualificationService _qualificationService;


        public IHtmlLocalizer T { get; set; }


        public CenterProfileQualificationsController(
            IAuthorizationService authorizationService,
            IHtmlLocalizer<CenterProfileQualificationsController> htmlLocalizer,
            INotifier notifier,
            IQualificationService qualificationService)
        {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _qualificationService = qualificationService;

            T = htmlLocalizer;
        }


        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            var settings = await _qualificationService.GetQualificationSettingsAsync();
            var viewModel = new CenterProfileQualificationSettingsViewModel();
            viewModel.UpdateViewModel(settings);

            AddQualificationsToViewData(settings);

            return View(viewModel);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        [RequestFormSizeLimit(6144, Order = 1)]
        [ValidateAntiForgeryToken(Order = 2)]
        public async Task<IActionResult> IndexPost(CenterProfileQualificationSettingsViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                await _qualificationService.UpdateQualificationsPerOccupationsAsync(viewModel);
                _notifier.Success(T["Sikeres mentés."]);

                return RedirectToAction(nameof(Index));
            }

            AddQualificationsToViewData(await _qualificationService.GetQualificationSettingsAsync());

            return View(viewModel);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            try
            {
                return View(await _qualificationService.GetQualificationAsync(id));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost, ActionName(nameof(Edit))]
        public async Task<IActionResult> EditPost(QualificationViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                await _qualificationService.UpdateQualificationAsync(viewModel);
                _notifier.Success(T["Sikeres mentés."]);

                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View(viewModel);
            }
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            try
            {
                await _qualificationService.DeleteQualificationAsync(id);
                _notifier.Success(T["Sikeres törlés."]);

                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }


        private void AddQualificationsToViewData(CenterProfileQualificationSettings settings)
        {
            ViewData["Qualifications"] = settings.Qualifications.Select(x =>
            {
                var qualificationViewModel = new QualificationViewModel();
                qualificationViewModel.UpdateViewModel(x);

                return qualificationViewModel;
            });
        }
    }
}
