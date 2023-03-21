using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers.Api
{
    [Authorize]
    [Route("api/center-profile-tools", Name = "CenterProfileEquipmentsApi")]
    public class CenterProfileEquipmentsApiController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICenterProfileService _centerProfileService;


        public CenterProfileEquipmentsApiController(
            IAuthorizationService authorizationService,
            ICenterProfileService centerProfileService)
        {
            _authorizationService = authorizationService;
            _centerProfileService = centerProfileService;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterProfileAccreditationConditionsSettings))
            {
                return Unauthorized();
            }

            var viewModel = new CenterProfileEquipmentsSettingsViewModel();
            viewModel.UpdateViewModel(await _centerProfileService.GetCenterProfileEquipmentSettingsAsync());

            return Ok(viewModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post([FromBody]CenterProfileEquipmentsSettingsViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageCenterProfileAccreditationConditionsSettings))
            {
                return Unauthorized();
            }

            var settings = new CenterProfileEquipmentsSettings();
            IdForEquipments(viewModel.Tools);
            IdForEquipments(viewModel.Laboratory);

            viewModel.UpdateModel(settings);

            await _centerProfileService.SetCenterProfileEquipmentSettingsAsync(settings);

            return Ok(viewModel);
        }


        private void IdForEquipments(IEnumerable<CenterProfileEquipmentViewModel> equipments)
        {
            foreach (var equipment in equipments)
            {
                if (string.IsNullOrEmpty(equipment.Id))
                {
                    equipment.Id = Guid.NewGuid().ToString().Replace("-", string.Empty);
                }
            }
        }
    }
}
