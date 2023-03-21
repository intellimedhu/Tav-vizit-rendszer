using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users.Models;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    /// <summary>
    /// Service to get the wider organization related properties and lists.
    /// </summary>
    public interface IOrganizationService
    {
        /// <summary>
        /// Gets the main properties of the organization.
        /// </summary>
        /// <returns></returns>
        Task<OrganizationViewModel> GetOrganizationAsync();

        /// <summary>
        /// Lists all organization units.
        /// </summary>
        /// <returns>A list of all organization units.</returns>
        Task<IEnumerable<OrganizationUnitPart>> ListOrganizationUnitsAsync();

        /// <summary>
        /// Lists the permitted organization units of an organization user profile.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <returns>A list of organization units.</returns>
        Task<IEnumerable<OrganizationUnitPart>> ListPermittedOrganizationUnitsAsync(OrganizationUserProfilePart profile);

        /// <summary>
        /// Gets the signed in organization unit of a user.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="user">The user.</param>
        /// <returns>The organization unit which the user is signed in.</returns>
        Task<OrganizationUnitPart> GetSignedInOrganizationUnitAsync(IServiceScope managersServiceScope, User user);

        /// <summary>
        /// Gets the organization unit by ID.
        /// </summary>
        /// <param name="contentItemId">The organization unit's id.</param>
        /// <returns>The organization unit.</returns>
        Task<OrganizationUnitPart> GetOrganizationUnitAsync(string contentItemId);

        Task SignInToOrganizationAsync(string organizationUnitId, User user);
    }
}
