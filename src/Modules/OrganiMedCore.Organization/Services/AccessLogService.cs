using IntelliMed.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Users.Models;
using OrganiMedCore.Organization.Models;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    public class AccessLogService : IAccessLogService
    {
        private readonly ILogger<AccessLogService> _logger;
        private readonly IOrganizationUserProfileService _organizationUserProfileService;


        public AccessLogService(ILogger<AccessLogService> logger, IOrganizationUserProfileService organizationUserProfileService)
        {
            _logger = logger;
            _organizationUserProfileService = organizationUserProfileService;
        }


        public async Task LogActivityAsync(IServiceScope managersServiceScope, User user, string message)
        {
            var signedInOrganizationUnitId = (await _organizationUserProfileService.GetOrganizationUserProfileAsync(managersServiceScope, user)).As<OrganizationUserProfilePart>().SignedInOrganizationUnitId;
            _logger.LogInformation($"User ID: {user.Id}. Signed in organization unit ID: {signedInOrganizationUnitId}. Message: {message}.");
        }

        public async Task LogContentActivityAsync(IServiceScope managersServiceScope, User user, Crud crud, ContentItem contentItem)
        {
            var signedInOrganizationUnitId = (await _organizationUserProfileService.GetOrganizationUserProfileAsync(managersServiceScope, user)).As<OrganizationUserProfilePart>().SignedInOrganizationUnitId;
            _logger.LogInformation($"User ID: {user.Id}. Signed in organization unit ID: {signedInOrganizationUnitId}. Activity type: {crud}. Content item ID: {contentItem.Id}. Content item type: {contentItem.ContentType}.");
        }
    }
}
