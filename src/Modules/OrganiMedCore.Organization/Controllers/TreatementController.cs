using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrganiMedCore.Organization.ActionFilters;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Models.Enums;
using OrganiMedCore.Organization.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Controllers
{
    [Authorize, ServiceFilter(typeof(OrganizationUnitActionFilter))]
    public class TreatementController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICheckInManager _checkInManager;
        private readonly IContentManager _contentManager;
        private readonly IOrganizationAuthorizationService _organizationAuthorizationService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public TreatementController(
            IAuthorizationService authorizationService,
            ICheckInManager checkInManager,
            IContentManager contentManager,
            IOrganizationAuthorizationService organizationAuthorizationService,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _authorizationService = authorizationService;
            _checkInManager = checkInManager; ;
            _contentManager = contentManager;
            _organizationAuthorizationService = organizationAuthorizationService;
            _sharedDataAccessorService = sharedDataAccessorService;
        }


        [Route("begin-treatement/{checkInId}")]
        public async Task<IActionResult> BeginTreatement(string checkInId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTreatmentWorkflow))
            {
                return Unauthorized();
            }

            var checkIn = await _contentManager.GetAsync(checkInId, ContentTypes.CheckIn);
            if (checkIn == null)
            {
                return NotFound();
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                if (!await _organizationAuthorizationService.AuthorizedToEditAsync(scope, User, checkIn))
                {
                    return new UnauthorizedResult();
                }
            }

            await _checkInManager.SetCheckInStatus(checkInId, CheckInStatuses.TreatmentInProgress);

            return View();
        }
    }
}
