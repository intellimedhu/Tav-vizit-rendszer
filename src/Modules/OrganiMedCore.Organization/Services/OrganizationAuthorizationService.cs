using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrganiMedCore.Core.Constants;
using OrganiMedCore.Organization.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    public class OrganizationAuthorizationService : IOrganizationAuthorizationService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IBetterUserService _betterUserService;
        private readonly IOrganizationUserProfileService _organizationUserProfileService;


        public OrganizationAuthorizationService(IAuthorizationService authorizationService, IBetterUserService betterUserService, IOrganizationUserProfileService organizationUserProfileService)
        {
            _authorizationService = authorizationService;
            _betterUserService = betterUserService;
            _organizationUserProfileService = organizationUserProfileService;
        }


        public async Task<bool> AuthorizedToEditAsync(IServiceScope managersServiceScope, ClaimsPrincipal user, ContentItem contentItem)
        {
            var authorizedToManagePatients = await _authorizationService.AuthorizeAsync(user, Permissions.ManagePatinets);

            // The content item is a patient profile.
            if (contentItem.ContentType == ContentTypes.EVisitPatientProfile)
            {
                return authorizedToManagePatients;
            }

            if (!authorizedToManagePatients)
            {
                return false;
            }
            // The content item is a standard item with MetaDataPart.
            var metaDataPart = contentItem.As<MetaDataPart>();
            if (metaDataPart != null)
            {
                var userEntity = await _betterUserService.ConvertUserAsync(user);
                var profile = await _organizationUserProfileService.GetOrganizationUserProfileAsync(managersServiceScope, userEntity);
                var profilePart = profile.As<OrganizationUserProfilePart>();
                return !string.IsNullOrEmpty(profilePart.SignedInOrganizationUnitId) &&
                    profilePart.SignedInOrganizationUnitId == metaDataPart.OrganizationUnitId;

                // TODO: Add check for házi beutaló.
            }

            // The content item is an exceptional type.
            // TODO: add exceptional types here.
            return false;
        }
    }
}
