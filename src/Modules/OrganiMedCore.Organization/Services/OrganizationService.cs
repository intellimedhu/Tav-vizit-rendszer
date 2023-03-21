using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Extensions;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Settings;
using OrganiMedCore.Organization.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.Organization.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IContentManager _contentManager;
        private readonly IOrganizationUserProfileService _organizationUserProfileService;
        private readonly ISiteService _siteService;
        private readonly ShellSettings _shellSettings;
        private readonly ISession _session;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public OrganizationService(
            IContentManager contentManager,
            ISiteService siteService,
            ShellSettings shellSettings,
            ISession session,
            IOrganizationUserProfileService organizationUserProfileService,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _contentManager = contentManager;
            _organizationUserProfileService = organizationUserProfileService;
            _siteService = siteService;
            _shellSettings = shellSettings;
            _session = session;
            _sharedDataAccessorService = sharedDataAccessorService;
        }


        public async Task<OrganizationViewModel> GetOrganizationAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var organizationSettings = siteSettings.As<OrganizationSettings>();
            if (organizationSettings == null) return null;

            return new OrganizationViewModel
            {
                Id = _shellSettings.Name,
                Name = siteSettings.SiteName,
                EesztId = organizationSettings.EesztId,
                EesztName = organizationSettings.EesztName
            };
        }

        public async Task<IEnumerable<OrganizationUnitPart>> ListOrganizationUnitsAsync()
        {
            return (await _session
                .Query<ContentItem, ContentItemIndex>(x => x.ContentType == ContentTypes.OrganizationUnit)
                .LatestAndPublished()
                .ListAsync())
                .Select(x => x.As<OrganizationUnitPart>());
        }

        public async Task<IEnumerable<OrganizationUnitPart>> ListPermittedOrganizationUnitsAsync(OrganizationUserProfilePart profile)
        {
            return (await ListOrganizationUnitsAsync())
                .Where(x => profile.PermittedOrganizationUnits.Contains(x.ContentItem.ContentItemId));
        }

        public async Task<OrganizationUnitPart> GetSignedInOrganizationUnitAsync(IServiceScope managersServiceScope, User user)
        {
            var profile = await _organizationUserProfileService.GetOrganizationUserProfileAsync(managersServiceScope, user);
            if (profile == null) return null;
            var profilePart = profile.As<OrganizationUserProfilePart>();

            var organizationUnit = await _session
                 .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentItemId == profilePart.SignedInOrganizationUnitId &&
                    x.ContentType == ContentTypes.OrganizationUnit)
                .LatestAndPublished()
                .FirstOrDefaultAsync();
            if (organizationUnit == null) return null;

            var part = organizationUnit.As<OrganizationUnitPart>();
            var organizationUnitId = part.ContentItem.ContentItemId;
            if (!profilePart.PermittedOrganizationUnits.Contains(organizationUnitId))
            {
                return null;
            }

            return part;
        }

        public async Task<OrganizationUnitPart> GetOrganizationUnitAsync(string id)
        {
            var organizationUnit = await _contentManager.GetAsync(id, ContentTypes.OrganizationUnit);

            return organizationUnit?.As<OrganizationUnitPart>();
        }

        public async Task SignInToOrganizationAsync(string organizationUnitId, User user)
        {
            ContentItem profile;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                profile = await _organizationUserProfileService.GetOrganizationUserProfileAsync(scope, user);
            }

            if (profile == null)
            {
                throw new NotFoundException();
            }

            var organizationsUnits = await ListPermittedOrganizationUnitsAsync(profile.As<OrganizationUserProfilePart>());
            if (!organizationsUnits.Any(x => x.ContentItem.ContentItem.ContentItemId == organizationUnitId))
            {
                throw new NotPermittedOrganizationUnitException();
            }

            if (profile.As<OrganizationUserProfilePart>().SignedInOrganizationUnitId != organizationUnitId)
            {
                var profileToSave = await _contentManager.GetAsync(profile.ContentItemId, ContentTypes.OrganizationUserProfile);
                profileToSave.Alter<OrganizationUserProfilePart>(x => x.SignedInOrganizationUnitId = organizationUnitId);

                await _contentManager.UpdateAsync(profileToSave);
            }
        }
    }
}
