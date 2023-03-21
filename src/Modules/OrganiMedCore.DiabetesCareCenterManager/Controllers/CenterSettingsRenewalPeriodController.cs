using IntelliMed.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Admin]
    [Authorize]
    public class CenterSettingsRenewalPeriodController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IClock _clock;
        private readonly INotifier _notifier;
        private readonly IRenewalPeriodSettingsService _renewalPeriodSettingsService;
        private readonly IRenewalPeriodService _renewalPeriodService;


        public IHtmlLocalizer T { get; set; }


        public CenterSettingsRenewalPeriodController(
            IAuthorizationService authorizationService,
            IClock clock,
            IHtmlLocalizer<CenterSettingsRenewalPeriodController> htmlLocalizer,
            INotifier notifier,
            IRenewalPeriodSettingsService renewalPeriodSettingsService,
            IRenewalPeriodService renewalPeriodService)
        {
            _authorizationService = authorizationService;
            _clock = clock;
            _notifier = notifier;
            _renewalPeriodSettingsService = renewalPeriodSettingsService;
            _renewalPeriodService = renewalPeriodService;

            T = htmlLocalizer;
        }


        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            return View(await _renewalPeriodSettingsService.ListRenewalSettingsAsync());
        }

        public async Task<IActionResult> Edit(Guid? id = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            try
            {
                return View(await _renewalPeriodSettingsService.GetRenewalSettingsAsync(id));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost, ActionName(nameof(Edit))]
        public async Task<IActionResult> EditPost(RenewalSettingsViewModel viewModel)
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
                await _renewalPeriodSettingsService.UpdateRenewalSettingsAsync(viewModel);
                _notifier.Success(T["Sikeres mentés"]);

                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (RenewalTimingOutOfDateRangeException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    T["Az időzítések mindegyikének a 'Megújítási időszak kezedete' és 'Ellenőrzési időszak kezdete' közé kell estie."].Value);

                return View(viewModel);
            }
            catch (RenewalTimingException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    T["A 'Megújítási időszak kezedete' korábbi kell legyen, mint az 'Ellenőrzési időszak kezdete', " +
                    "és az 'Ellenőrzési időszak kezdete' korábbi kell legyen, mint az 'Ellenőrzési időszak vége'."].Value);

                return View(viewModel);
            }
            catch (ArgumentNullException ex)
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
                await _renewalPeriodSettingsService.DeleteRenewalSettingsAsync(id);
                _notifier.Success(T["Sikeres törlés"]);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> ResetCenterProfiles()
        {
            try
            {
                var affected = await _renewalPeriodService.ResetCenterProfileStatusesAsync(_clock.UtcNow, true);

                if (affected > 0)
                {
                    _notifier.Success(T["A művelet sikeres volt: összesen {0} db adatlap lett megnyitva.", affected]);
                }
                else
                {
                    _notifier.Information(T["Nincs egyetlen olyan adatlap sem, amelynél engedélyezett az adatlap megnyitása."]);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                _notifier.Error(T["A művelet nem sikerült."]);

                throw;
            }
        }
    }
}
