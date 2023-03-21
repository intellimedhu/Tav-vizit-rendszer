using IntelliMed.Core.Constants;
using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrganiMedCore.DiabetesCareCenter.Core.Extension;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/center-profile-init", Name = "CenterProfileInit")]
    public class CenterProfileInitApiController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ShellSettings _shellSettings;


        public IHtmlLocalizer T { get; set; }


        public CenterProfileInitApiController(
            IHtmlLocalizer<CenterProfileInitApiController> htmlLocalizer,
            IServiceProvider serviceProvider,
            ISharedDataAccessorService sharedDataAccessorService,
            ShellSettings shellSettings)
        {
            _serviceProvider = serviceProvider;
            _sharedDataAccessorService = sharedDataAccessorService;
            _shellSettings = shellSettings;

            T = htmlLocalizer;
        }


        [HttpGet, Route("")]
        public async Task<IActionResult> Get()
        {
            var equipmentSettings = await GetEquipmentsSettingsAsync();

            return Ok(new
            {
                AccreditationStatuses = AccreditationStatusCaptions.GetLocalizedValues(T)
                    .Select(x => new KeyValuePair<int, string>((int)x.Key, x.Value)),
                CenterTypes = CenterTypeCaptions.GetLocalizedValues(T)
                     .Select(x => new KeyValuePair<int, string>((int)x.Key, x.Value)),
                Days = DayCaptions.GetLocalizedValues(T)
                    .Select(x => new KeyValuePair<int, string>((int)x.Key, x.Value)),
                Occupations = OccupationExtensions.GetLocalizedValues(T)
                    .Select(x => new KeyValuePair<int, string>((int)x.Key, x.Value)),
                equipmentSettings.Laboratory,
                equipmentSettings.Tools,
                ColleagueStatuses = ColleagueStatusExtensions.GetLocalizedValues(T)
                    .Select(value => new
                    {
                        value.Key,
                        value.Value
                    }),
                QualificationStateCaptions = QualificationStateCaptions.GetLocalizedValues(T)
                    .Select(value => new
                    {
                        value.Key,
                        value.Value
                    }),
                ColleagueStatusZones = new
                {
                    Active = ColleagueStatusExtensions.GreenZone,
                    Pending = ColleagueStatusExtensions.PendingZone,
                    Removed = ColleagueStatusExtensions.RemovedZone
                }
            });
        }


        private async Task<CenterProfileEquipmentsSettingsViewModel> GetEquipmentsSettingsAsync()
        {
            CenterProfileEquipmentsSettings settings = null;
            if (_shellSettings.Name == WellKnownNames.DiabetesCareCenterManagerTenantName)
            {
                settings = await _serviceProvider.GetRequiredService<ICenterProfileService>()
                    .GetCenterProfileEquipmentSettingsAsync();
            }
            else
            {
                using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
                {
                    settings = await scope.ServiceProvider.GetRequiredService<ICenterProfileService>()
                        .GetCenterProfileEquipmentSettingsAsync();
                }
            }

            var viewModel = new CenterProfileEquipmentsSettingsViewModel();
            viewModel.UpdateViewModel(settings);

            return viewModel;
        }
    }
}
