using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrganiMedCore.Core.Constants;
using OrganiMedCore.Core.Indexes;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrganiMedCore.Core.Services
{
    public class EVisitOrganizationUserProfileService : IEVisitOrganizationUserProfileService
    {
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly UserManager<IUser> _userManager;
        private readonly ISession _session;
        private readonly IUserService _userService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly IContentManager _contentManager;


        public EVisitOrganizationUserProfileService(
            ISharedDataAccessorService sharedDataAccessorService,
            UserManager<IUser> userManager,
            ISession session,
            IUserService userService,
            IPasswordGeneratorService passwordGeneratorService,
            IContentManager contentManager)
        {
            _sharedDataAccessorService = sharedDataAccessorService;
            _userManager = userManager;
            _session = session;
            _userService = userService;
            _passwordGeneratorService = passwordGeneratorService;
            _contentManager = contentManager;
        }


        public async Task<IEnumerable<ContentItem>> ListAsync(IServiceScope managersServiceScope)
        {
            var ids = (await _session.Query<AssignedEVisitOrganizationProfile>().ListAsync()).Select(x => x.EVisitOrganizationUserProfileId);

            var managerSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();

            return await managerSession
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentItemId.IsIn(ids) &&
                    x.ContentType == ContentTypes.EVisitOrganizationUserProfile)
                .LatestAndPublished()
                .ListAsync();
        }

        public async Task<ContentItem> InitializeAsync(IServiceScope managersServiceScope)
            => await managersServiceScope.ServiceProvider.GetRequiredService<IContentManager>()
                .NewAsync(ContentTypes.EVisitOrganizationUserProfile);

        public async Task<ContentItem> GetAsync(IServiceScope managersServiceScope, string contentItemId)
            => await managersServiceScope.ServiceProvider.GetRequiredService<IContentManager>()
                .GetAsync(contentItemId, ContentTypes.EVisitOrganizationUserProfile);

        public async Task<ContentItem> GetNewVerisonAsync(IServiceScope managersServiceScope, string contentItemId)
            => await managersServiceScope.ServiceProvider.GetRequiredService<IContentManager>()
                .GetNewVersionAsync(contentItemId, ContentTypes.EVisitOrganizationUserProfile);

        public async Task<ContentItem> GetByIdentifierAsync(IServiceScope managersServiceScope, string identifier, OrganizationUserIdentifierTypes identifierType)
        {
            var managerSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();

            IQuery<ContentItem, EVisitOrganizationUserProfilePartIndex> query;

            switch (identifierType)
            {
                case OrganizationUserIdentifierTypes.Email:
                    query = managerSession.Query<ContentItem, EVisitOrganizationUserProfilePartIndex>(
                        x => x.Email == identifier);
                    break;
                case OrganizationUserIdentifierTypes.StampNumber:
                    query = managerSession.Query<ContentItem, EVisitOrganizationUserProfilePartIndex>(
                        x => x.StampNumber == identifier);
                    break;
                default:
                    return null;
            }

            var profile = await query
                //.LatestAndPublished()
                .FirstOrDefaultAsync();

            return profile != null && profile.ContentType == ContentTypes.EVisitOrganizationUserProfile ? profile : null;
        }

        public async Task AssignToTenantAsync(
            IServiceScope managersServiceScope,
            int sharedUserId,
            string tenantName,
            IList<string> tenantRoles,
            bool tenantIsOrganization,
            Action<string, string> reportError)
        {
            sharedUserId.ThrowIfNull();
            tenantName.ThrowIfNullOrEmpty();

            var managersUserManager = managersServiceScope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var eVisitIUser = await managersUserManager.FindByIdAsync(sharedUserId.ToString());
            if (eVisitIUser == null)
            {
                reportError("", "Az OrganiMed felhasználó nem található.");
                return;
            }
            var eVisitUser = (User)eVisitIUser;

            ContentItem organizationProfile = null;
            if (tenantIsOrganization)
            {
                var managersSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();
                organizationProfile = await managersSession.Query<ContentItem, EVisitOrganizationUserProfilePartIndex>(x => x.SharedUserId == sharedUserId)
                    .LatestAndPublished()
                    .FirstOrDefaultAsync();

                if (organizationProfile == null || organizationProfile.ContentType != ContentTypes.EVisitOrganizationUserProfile)
                {
                    reportError("", "Az eVisit felhasználói profil nem található.");
                    return;
                }
            }

            // Enabling login on the organization for the user.
            eVisitUser.Alter<EVisitUser>(nameof(EVisitUser), x =>
            {
                x.IsEVisitUser = true;
                x.PermittedTenans.Add(tenantName);
            });
            _sharedDataAccessorService.Save(managersServiceScope, eVisitIUser);

            // Creating local user.
            if (await _userManager.FindByNameAsync(sharedUserId.ToString()) == null)
            {
                // Shared user ID is stored in the tenant's username.
                var tenantUser = await _userService.CreateUserAsync(new User
                {
                    UserName = eVisitUser.Id.ToString(),
                    Email = eVisitUser.Email,
                    RoleNames = tenantRoles
                },
                _passwordGeneratorService.GenerateRandomPassword(16),
                reportError);
            }

            if (tenantIsOrganization)
            {
                // Adding local record to indicate that the organization profile is now added to the organization.
                var record = await _session
                    .Query<AssignedEVisitOrganizationProfile, AssignedEVisitOrganizationProfileIndex>(x =>
                        x.EVisitOrganizationUserProfileId == organizationProfile.ContentItem.ContentItemId)
                    .FirstOrDefaultAsync();
                if (record == null)
                {
                    _session.Save(new AssignedEVisitOrganizationProfile { EVisitOrganizationUserProfileId = organizationProfile.ContentItemId });
                }
            }
        }

        public async Task RemoveFromOrganizationAsync(IServiceScope managersServiceScope, int sharedUserId, string organizationId, Action<string, string> reportError)
        {
            sharedUserId.ThrowIfNull();
            organizationId.ThrowIfNullOrEmpty();

            var managersUserManager = managersServiceScope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
            var eVisitIUser = await managersUserManager.FindByIdAsync(sharedUserId.ToString());
            if (eVisitIUser == null)
            {
                reportError("", "Az OrganiMed felhasználó nem található.");
                return;
            }
            var eVisitUser = (User)eVisitIUser;

            var managersSession = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();
            var profile = await managersSession
                .Query<ContentItem, EVisitOrganizationUserProfilePartIndex>(x => x.SharedUserId == sharedUserId)
                .LatestAndPublished()
                .FirstOrDefaultAsync();

            if (profile == null || profile.ContentType != ContentTypes.EVisitOrganizationUserProfile)
            {
                reportError("", "Az eVisit felhasználói profil nem található.");
                return;
            }

            // Disabling login on the organization for the user.
            eVisitUser.Alter<EVisitUser>(nameof(EVisitUser), x =>
            {
                x.PermittedTenans.Remove(organizationId);
            });
            _sharedDataAccessorService.Save(managersServiceScope, eVisitIUser);

            // Removing local user, profile and indicator record.
            var localUser = await _userManager.FindByNameAsync(sharedUserId.ToString());
            if (localUser != null)
            {
                await _userManager.DeleteAsync(localUser);
            }
            await _contentManager.RemoveAsync(profile);

            var record = await _session
                .Query<AssignedEVisitOrganizationProfile, AssignedEVisitOrganizationProfileIndex>(x =>
                    x.EVisitOrganizationUserProfileId == profile.ContentItem.ContentItemId)
                .FirstOrDefaultAsync();
            if (record != null)
            {
                _session.Delete(record);
            }
        }

        public async Task<ContentItem> GetEVisitOrganizationUserProfileBySharedUserIdAsync(IServiceScope managersServiceScope, int sharedUserId)
        {
            var session = managersServiceScope.ServiceProvider.GetRequiredService<ISession>();

            return await session
                .Query<ContentItem, EVisitOrganizationUserProfilePartIndex>(x => x.SharedUserId == sharedUserId)
                .FirstOrDefaultAsync();
        }
    }
}
