using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Users.Models;
using OrganiMedCore.Core.Models.Enums;
using OrganiMedCore.Organization.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    /// <summary>
    /// A service to manage (mostly get and convert) local organization user profiles.
    /// </summary>
    public interface IOrganizationUserProfileService
    {
        /// <summary>
        /// Gets the local organization user profile of to the user.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="user">The user.</param>
        /// <returns>The local organization user profile.</returns>
        Task<ContentItem> GetOrganizationUserProfileAsync(IServiceScope managersServiceScope, User user);

        /// <summary>
        /// Gets the local user related to the local organizaion user profile.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="profile">The local organizaion user profile.</param>
        /// <returns>The local user.</returns>
        Task<User> GetLocalUserAsync(IServiceScope managersServiceScope, ContentItem profile);

        /// <summary>
        /// Gets the organization profile of all users in the doctor role. It also filters on permitted organization unit.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="organizationUnitId">The ID of the organization unit.</param>
        /// <returns>A list of doctors.</returns>
        Task<IEnumerable<DoctorViewModel>> GetDoctors(IServiceScope managersServiceScope, string organizationUnitId);

        Task<ContentItem> GetOrganizationUserProfileAsync(
            string organizationUserProfileId,
            string eVisitProfileContentItemId,
            OrganizationUserProfileTypes? organizationUserProfileType,
            bool displayOnly);

        Task<ContentItem> GetOrganizationUserProfileAsync(string eVisitProfileContentItemId, OrganizationUserProfileTypes? organizationUserProfileType);

        Task SaveOrganizationUserProfileAsync(ContentItem organizationProfile, bool isNew);

        Task AddPermittedOrganizationUnitsAsync(ContentItem contentItemOganizationUserProfile, IEnumerable<string> organizationUnitIds);
    }
}
