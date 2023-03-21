using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Core.Services
{
    /// <summary>
    /// Service for managing the shared eVisit patient profiles.
    /// </summary>
    public interface IEVisitPatientProfileService
    {
        /// <summary>
        /// Gets the patient profile by an identifier.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="identifierValue">The value of the identifier.</param>
        /// <param name="identifierType">The type of the identifier.</param>
        /// <returns>The patient profile stored in the database.</returns>
        Task<ContentItem> GetByIdentifierAsync(IServiceScope managersServiceScope, string identifierValue, PatientIdentifierTypes identifierType);

        /// <summary>
        /// Initializes a new patient profile.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <returns>A new patient profile.</returns>
        Task<ContentItem> InitializeAsync(IServiceScope managersServiceScope);

        /// <summary>
        /// Gets the existing patient profile.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="contentItemId">The ID of the patient profile content item.</param>
        /// <returns>The patient profile stored in the database.</returns>
        Task<ContentItem> GetAsync(IServiceScope managersServiceScope, string contentItemId);

        /// <summary>
        /// Gets a new version of the existing patient profile.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="contentItemId">The ID of the patient profile content item.</param>
        /// <returns>The new version of the patient profile stored in the database.</returns>
        Task<ContentItem> GetNewVersionAsync(IServiceScope managersServiceScope, string contentItemId);

        /// <summary>
        /// Performs full text search on the evisitpatients lucene index.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="filter">A filter which will be used against the stored properties.</param>
        /// <param name="size">The size of the result set.</param>
        /// <returns>A list of patient profiles.</returns>
        Task<IEnumerable<ContentItem>> SearchAsync(IServiceScope managersServiceScope, EVisitPatientProfileFilter filter, int size);

        /// <summary>
        /// Lists all organization profiles by IDs.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="ids">A list of shared eVisit profile IDs.</param>
        /// <returns>A list of patient profiles.</returns>
        Task<IEnumerable<ContentItem>> GetByIds(IServiceScope managersServiceScope, IEnumerable<string> ids);
    }
}
