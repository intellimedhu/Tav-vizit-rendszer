using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganiMedCore.Organization.Services;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Controllers.Api
{
    [Authorize, ApiController, Route("api/doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrganizationUserProfileService _organizationUserProfileService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public DoctorController(
            IAuthorizationService authorizationService,
            IOrganizationUserProfileService organizationUserProfileService,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _authorizationService = authorizationService;
            _organizationUserProfileService = organizationUserProfileService;
            _sharedDataAccessorService = sharedDataAccessorService;
        }


        [HttpGet]
        [Route("", Name = "GetDoctorsByOrganizationUnitId")]
        public async Task<IActionResult> GetDoctorsByOrganizationUnitId(string organizationUnitId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageReception))
            {
                return Unauthorized();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var doctors = await _organizationUserProfileService.GetDoctors(scope, organizationUnitId);

                return new ObjectResult(doctors.Select(x => new
                {
                    text = $"{x.EVisitOrganizationUserProfilePart.Name} - {x.EVisitOrganizationUserProfilePart.StampNumber}",
                    value = x.EVisitOrganizationUserProfilePart.ContentItem.ContentItemId
                }));
            }
        }
    }
}
