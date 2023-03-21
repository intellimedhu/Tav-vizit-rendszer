using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrganiMedCore.Organization.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    /// <summary>
    /// This service partly depends on that the content has a <see cref="MetaDataPart"/>.
    /// </summary>
    public interface IOrganizationAuthorizationService
    {
        /// <summary>
        /// Determines whether the user is authorized to edit that eVisit content item.
        /// TODO: rethink this in v2.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="user">The user who wants to edit the content item.</param>
        /// <param name="contentItem">The content item to edit.</param>
        /// <returns>Whether the user is authorized to edit the content item.</returns>
        Task<bool> AuthorizedToEditAsync(IServiceScope managersServiceScope, ClaimsPrincipal user, ContentItem contentItem);
    }
}
