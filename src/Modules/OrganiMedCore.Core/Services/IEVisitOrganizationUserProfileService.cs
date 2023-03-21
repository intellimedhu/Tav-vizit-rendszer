using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrganiMedCore.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Core.Services
{
    /// <summary>
    /// Service for managing the shared eVisit organization user profiles.
    /// </summary>
    public interface IEVisitOrganizationUserProfileService
    {
        /// <summary>
        /// Initializes a new organization profile.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <returns>A new organization profile.</returns>
        Task<ContentItem> InitializeAsync(IServiceScope managersServiceScope);

        /// <summary>
        /// Gets the existing organization profile.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="contentItemId">The ID of the organization profile content item.</param>
        /// <returns>The organization profile stored in the database.</returns>
        Task<ContentItem> GetAsync(IServiceScope managersServiceScope, string contentItemId);

        /// <summary>
        /// Gets a new version of the existing organization profile.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="contentItemId">The ID of the organization profile content item.</param>
        /// <returns>The new version of the organization profile stored in the database.</returns>
        Task<ContentItem> GetNewVerisonAsync(IServiceScope managersServiceScope, string contentItemId);

        /// <summary>
        /// Lists the latest version of all organization profiles.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <returns>A list of organization profiles.</returns>
        Task<IEnumerable<ContentItem>> ListAsync(IServiceScope managersServiceScope);

        /// <summary>
        /// Gets the organization profile by an identifier.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="identifierValue">The value of the identifier.</param>
        /// <param name="identifierType">The type of the identifier.</param>
        /// <returns>The organization profile stored in the database.</returns>
        Task<ContentItem> GetByIdentifierAsync(IServiceScope managersServiceScope, string identifierValue, OrganizationUserIdentifierTypes identifierType);

        /// <summary>
        /// Adds the organization profile to the organization tenant. So the organization profile will be listed
        /// in the organization's lists and the user will be able to login to that organization.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="sharedUserId">The ID of the shared user which will be added.</param>
        /// <param name="tenantName">The technical name of the organization to add the user.</param>
        /// <param name="tenantRoles">The roles to assign to that user.</param>
        /// <param name="tenantIsOrganization">Should be set to True if the tenant is an organization.</param>
        /// <param name="reportError">The error reporting action.</param>
        Task AssignToTenantAsync(
            IServiceScope managersServiceScope,
            int sharedUserId,
            string tenantName,
            IList<string> tenantRoles,
            bool tenantIsOrganization,
            Action<string, string> reportError);

        /// <summary>
        /// Removes the organization profile from the organization tenant. So the organization profile will be removed
        /// from the organization's lists and the user won't be able to login to that organization.
        /// organization tenant.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="sharedUserId">The ID of the shared user which will be removed.</param>
        /// <param name="organizationId">The ID of the organization (tenant name) from remove the user.</param>
        /// <param name="reportError">The error reporting action.</param>
        Task RemoveFromOrganizationAsync(IServiceScope managersServiceScope, int sharedUserId, string organizationId, Action<string, string> reportError);

        Task<ContentItem> GetEVisitOrganizationUserProfileBySharedUserIdAsync(IServiceScope managersServiceScope, int sharedUserId);
    }
}
