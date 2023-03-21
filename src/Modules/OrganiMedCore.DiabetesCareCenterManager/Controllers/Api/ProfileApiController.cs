using IntelliMed.DokiNetIntegration.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.ModelBinding;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.Login.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/profile", Name = "ProfileApi")]
    public class ProfileApiController : Controller, IUpdateModel
    {
        private readonly ICenterProfileService _centerProfileService;
        private readonly IDiabetesUserProfileService _diabetesUserProfileService;
        private readonly ILogger _logger;
        private readonly ISharedUserService _sharedUserService;


        public ProfileApiController(
            ICenterProfileService centerProfileService,
            IDiabetesUserProfileService diabetesUserProfileService,
            ILogger<ProfileApiController> logger,
            ISharedUserService sharedUserService)
        {
            _centerProfileService = centerProfileService;
            _diabetesUserProfileService = diabetesUserProfileService;
            _logger = logger;
            _sharedUserService = sharedUserService;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            DokiNetMember dokiNetMember = null;
            try
            {
                dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();

                var viewModel = await _diabetesUserProfileService.InitializeProfileEditorAsync(dokiNetMember.MemberRightId);

                return Ok(viewModel);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"DokiNetService.GetDokiNetMemberById failed. MRID:{dokiNetMember?.MemberRightId}");

                return NotFound();
            }
            catch (UserHasNoMemberRightIdException)
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post([FromBody]DiabetesUserProfilePartViewModel viewModel)
        {
            try
            {
                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();

                await _diabetesUserProfileService.UpdateProfileAsync(dokiNetMember.MemberRightId, viewModel, this);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (dokiNetMember.DiabetLicenceNumber != viewModel.DiabetLicenceNumber)
                {
                    await _sharedUserService.UpdateAndGetCurrentUsersDokiNetMemberDataAsync();
                }

                // Recalculate accreditation statuses where necessary.
                await _centerProfileService.OnUserProfilesUpdatedAsync(new[] { dokiNetMember.MemberRightId });

                return Ok();
            }
            catch (UserHasNoMemberRightIdException)
            {
                return Unauthorized();
            }
        }

        [HttpPut]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Put([FromQuery(Name = "o")]Occupation occupation, [FromBody]DiabetesUserProfilePartViewModel viewModel)
        {
            try
            {
                var dokiNetMember = await _sharedUserService.GetCurrentUsersDokiNetMemberAsync();

                await _diabetesUserProfileService.UpdatePartialProfileAsync(occupation, dokiNetMember, viewModel, this);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (dokiNetMember.DiabetLicenceNumber != viewModel.DiabetLicenceNumber)
                {
                    await _sharedUserService.UpdateAndGetCurrentUsersDokiNetMemberDataAsync();
                }

                // Recalculate accreditation statuses where necessary.
                await _centerProfileService.OnUserProfilesUpdatedAsync(new[] { dokiNetMember.MemberRightId });

                return Ok();
            }
            catch (UserHasNoMemberRightIdException)
            {
                return Unauthorized();
            }
        }
    }
}
