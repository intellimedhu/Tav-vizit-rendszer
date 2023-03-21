using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers.Api
{
    [Authorize, Route("api/settlements", Name = "SettlementsApi")]
    public class SettlementsApiController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISettlementService _settlementService;


        public SettlementsApiController(IAuthorizationService authorizationService, ISettlementService settlementService)
        {
            _authorizationService = authorizationService;
            _settlementService = settlementService;
        }


        [HttpGet]
        public async Task<IActionResult> Get(int territoryId, int page = 0, string q = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageZipCodes))
            {
                return Unauthorized();
            }

            return Ok(new SettlementEditResponseViewModel()
            {
                TotalCount = await _settlementService.GetSettlementsCountAsync(territoryId, q),
                Settlements = (await _settlementService.GetSettlementsAsync(territoryId, page, q))
                    .Select(x => SettlementEditViewModel.ToViewModel(x))
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post([FromQuery]int territoryId, [FromBody]SettlementEditViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageZipCodes))
            {
                return Unauthorized();
            }

            var model = SettlementEditViewModel.ToModel(viewModel);
            model.TerritoryId = territoryId;

            await _settlementService.SaveSettlementAsync(model);

            return Ok(model);
        }

        [HttpDelete]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageZipCodes))
            {
                return Unauthorized();
            }

            await _settlementService.DeleteSettlementAsync(id);

            return Ok();
        }
    }
}
