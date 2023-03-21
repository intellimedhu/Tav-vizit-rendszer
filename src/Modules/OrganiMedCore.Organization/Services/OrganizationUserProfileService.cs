using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Users;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrganiMedCore.Core.Indexes;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Models.Enums;
using OrganiMedCore.Core.Services;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrganiMedCore.Organization.Services
{
    public class OrganizationUserProfileService : IOrganizationUserProfileService
    {
        private readonly IContentManager _contentManager;
        private readonly IEVisitOrganizationUserProfileService _eVisitOrganizationUserProfileService;
        private readonly ILookupNormalizer _lookupNormalizer;
        private readonly ISession _session;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly IUserTypeService _userTypeService;
        private readonly UserManager<IUser> _userManager;


        public OrganizationUserProfileService(
            IContentManager contentManager,
            IEVisitOrganizationUserProfileService eVisitOrganizationUserProfileService,
            ILookupNormalizer lookupNormalizer,
            ISession session,
            ISharedDataAccessorService sharedDataAccessorService,
            IUserTypeService userTypeService,
            UserManager<IUser> userManager)
        {
            _contentManager = contentManager;
            _eVisitOrganizationUserProfileService = eVisitOrganizationUserProfileService;
            _lookupNormalizer = lookupNormalizer;
            _session = session;
            _sharedDataAccessorService = sharedDataAccessorService;
            _userManager = userManager;
            _userTypeService = userTypeService;
        }


        public async Task<ContentItem> GetOrganizationUserProfileAsync(IServiceScope managersServiceScope, User localUser)
        {
            if (!_userTypeService.IsOrganizationUser(localUser))
            {
                return null;
            }

            if (!int.TryParse(localUser.UserName, out var sharedUserId))
            {
                return null;
            }

            var eVisitProfile = await _sharedDataAccessorService
                .Query<ContentItem, EVisitOrganizationUserProfilePartIndex>(managersServiceScope, x => x.SharedUserId == sharedUserId)
                .LatestAndPublished()
                .FirstOrDefaultAsync();

            if (eVisitProfile == null)
            {
                return null;
            }

            var profile = await _session
                .Query<ContentItem, OrganizationUserProfilePartIndex>(x => x.EVisitOrganizationUserProfileId == eVisitProfile.ContentItemId)
                .LatestAndPublished()
                .FirstOrDefaultAsync();

            return profile;
        }

        public async Task<User> GetLocalUserAsync(IServiceScope managersServiceScope, ContentItem profile)
        {
            var eVisitProfileId = profile.As<OrganizationUserProfilePart>().EVisitOrganizationUserProfileId;
            var eVisitProfile = await _eVisitOrganizationUserProfileService.GetAsync(managersServiceScope, eVisitProfileId);

            if (eVisitProfile == null)
            {
                return null;
            }

            var localUserName = eVisitProfile.As<EVisitOrganizationUserProfilePart>().SharedUserId.ToString();

            return (User)await _userManager.FindByNameAsync(localUserName);
        }

        public async Task<IEnumerable<DoctorViewModel>> GetDoctors(IServiceScope managersServiceScope, string organizationUnitId)
        {
            organizationUnitId.ThrowIfNullOrEmpty();

            // Get local users with Doctor role.
            // TODO: check if NormalizeName() works
            var normalizedRoleName = _lookupNormalizer.NormalizeName(OrganizationRoleNames.Doctor);
            var localUserNames = (await _session
                .Query<User, UserByRoleNameIndex>(u => u.RoleName == normalizedRoleName)
                .ListAsync())
                .Select(x => x.UserName);

            var sharedUserIds = new List<int>();
            foreach (var localUserName in localUserNames)
            {
                if (int.TryParse(localUserName, out var id))
                {
                    sharedUserIds.Add(id);
                }
            }

            // Get EVisitOrganizationUserProfiles by SharedUserId.
            var managersSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();
            var eVisitProfiles = await managersSession
                .Query<ContentItem, EVisitOrganizationUserProfilePartIndex>(x => x.SharedUserId.IsIn(sharedUserIds))
                .LatestAndPublished()
                .ListAsync();
            var eVisitProfileIds = eVisitProfiles.Select(x => x.ContentItemId);

            // Get OrganizationUserProfiles by EVisitOrganizationUserProfileId.
            var profiles = (await _session
                .Query<ContentItem, OrganizationUserProfilePartIndex>(x => x.EVisitOrganizationUserProfileId.IsIn(eVisitProfileIds))
                .LatestAndPublished()
                .ListAsync())
                .Where(x => x.As<OrganizationUserProfilePart>().PermittedOrganizationUnits.Contains(organizationUnitId));

            // Construct viewmodel.
            var viewModel = new List<DoctorViewModel>();
            foreach (var profile in profiles)
            {
                var evisitProfile = eVisitProfiles.Where(x => x.ContentItemId == profile.As<OrganizationUserProfilePart>().EVisitOrganizationUserProfileId).FirstOrDefault();
                if (evisitProfile != null)
                {
                    viewModel.Add(new DoctorViewModel
                    {
                        EVisitOrganizationUserProfilePart = evisitProfile.As<EVisitOrganizationUserProfilePart>(),
                        OrganizationUserProfilePart = profile.As<OrganizationUserProfilePart>()
                    });
                }
            }

            return viewModel;
        }

        public async Task<ContentItem> GetOrganizationUserProfileAsync(
            string organizationUserProfileId,
            string eVisitProfileContentItemId,
            OrganizationUserProfileTypes? organizationUserProfileType,
            bool displayOnly)
        {
            var isNew = string.IsNullOrEmpty(organizationUserProfileId);

            var contentItem = isNew
                ? await _contentManager.NewAsync(ContentTypes.OrganizationUserProfile)
                : displayOnly
                    ? await _contentManager.GetAsync(organizationUserProfileId, ContentTypes.OrganizationUserProfile)
                    : await _contentManager.GetNewVersionAsync(organizationUserProfileId, ContentTypes.OrganizationUserProfile);

            if (contentItem == null || (!isNew && contentItem.As<OrganizationUserProfilePart>().EVisitOrganizationUserProfileId != eVisitProfileContentItemId))
            {
                throw new NotFoundException("Az intézményi profil nem található");
            }

            contentItem.Alter<OrganizationUserProfilePart>(part =>
            {
                if (string.IsNullOrEmpty(contentItem.As<OrganizationUserProfilePart>().EVisitOrganizationUserProfileId))
                {
                    part.EVisitOrganizationUserProfileId = eVisitProfileContentItemId;
                }

                if (isNew)
                {
                    part.OrganizationUserProfileType = organizationUserProfileType;
                }
            });

            return contentItem;
        }

        public async Task<ContentItem> GetOrganizationUserProfileAsync(
            string eVisitProfileContentItemId,
            OrganizationUserProfileTypes? organizationUserProfileType)
        {
            eVisitProfileContentItemId.ThrowIfNull();

            var contentItem = await _session
                .Query<ContentItem, OrganizationUserProfilePartIndex>(index => index.EVisitOrganizationUserProfileId == eVisitProfileContentItemId)
                .FirstOrDefaultAsync();

            if (contentItem == null)
            {
                contentItem = await _contentManager.NewAsync(ContentTypes.OrganizationUserProfile);
                contentItem.Alter<OrganizationUserProfilePart>(part =>
                {
                    part.EVisitOrganizationUserProfileId = eVisitProfileContentItemId;
                    part.OrganizationUserProfileType = organizationUserProfileType;
                });
            }

            await _contentManager.CreateAsync(contentItem);

            return contentItem;
        }

        public async Task SaveOrganizationUserProfileAsync(ContentItem organizationProfile, bool isNew)
        {
            if (isNew)
            {
                await _contentManager.CreateAsync(organizationProfile);
            }
            else
            {
                await _contentManager.PublishAsync(organizationProfile);
            }
        }

        public async Task AddPermittedOrganizationUnitsAsync(ContentItem contentItemOganizationUserProfile, IEnumerable<string> organizationUnitIds)
        {
            contentItemOganizationUserProfile.ThrowIfNull();
            organizationUnitIds.ThrowIfNull();

            contentItemOganizationUserProfile.Alter<OrganizationUserProfilePart>(part =>
            {
                foreach (var id in organizationUnitIds)
                {
                    part.PermittedOrganizationUnits.Add(id);
                }
            });

            await _contentManager.UpdateAsync(contentItemOganizationUserProfile);
        }
    }
}
