using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrganiMedCore.Organization.Models.Enums;
using OrganiMedCore.Organization.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Services
{
    /// <summary>
    /// Manages the patient check-in process.
    /// </summary>
    public interface ICheckInManager
    {
        /// <summary>
        /// Checks in the patient to the selected organization unit.
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="eVisitPatientProfileId">The ID of the shared eVisit patient profile.</param>
        /// <param name="organizationUnitId">The ID of the organization unit to check in the patient.</param>
        /// <param name="eVisitOrganizationUserProfileId">The doctor's profile ID. He/she will tend the patient.</param>
        Task CheckInPatient(IServiceScope managersServiceScope, string eVisitPatientProfileId, string organizationUnitId, string eVisitOrganizationUserProfileId);

        /// <summary>
        /// Gets the check-ins related to the patient and the organization unit. There can be multiple check-ins but only one with waiting status.
        /// </summary>
        /// <param name="eVisitPatientProfileId">The ID of the shared eVisit patient profile.</param>
        /// <param name="organizationUnitId">The ID of the organization unit the patient checked-in.</param>
        /// <param name="checkInDateUtc">The date of the check-ins.</param>
        /// <returns>A list of check-ins.</returns>
        Task<IEnumerable<ContentItem>> GetCheckInsAsync(string eVisitPatientProfileId, string organizationUnitId, DateTime checkInDateUtc);

        /// <summary>
        /// Gets the check-ins related to the organization unit (or all in the organization).
        /// </summary>
        /// <param name="checkInDateUtc">The date of the check-ins.</param>
        /// <param name="organizationUnitId">The ID of the organization unit.</param>
        /// <returns>A list of check-ins.</returns>
        Task<IEnumerable<ContentItem>> GetCheckInsAsync(DateTime checkInDateUtc, string organizationUnitId = "");

        /// <summary>
        /// Gets the checked-in patients related to the organization unit (or all in the organization).
        /// </summary>
        /// <param name="managersServiceScope">The service scope of the manager tenant.</param>
        /// <param name="checkInDateUtc">The date of the check-ins.</param>
        /// <param name="organizationUnitId">The ID of the organization unit.</param>
        /// <returns>A viewmodel which contains a list of items that contains the check-in, the related doctor, patient and organization unit.</returns>
        Task<DailyListViewModel> GetCheckedInPatientsAsync(IServiceScope managersServiceScope, DateTime checkInDateUtc, string organizationUnitId = "");

        /// <summary>
        /// Sets the check-in status of a check-in. It can be set only in a particular order so use this method all time to set it.
        /// </summary>
        /// <param name="checkInId">The ID of the check-in.</param>
        /// <param name="checkInStatus">The status to set.</param>
        /// <returns>Whether the set was successful.</returns>
        Task<bool> SetCheckInStatus(string checkInId, CheckInStatuses checkInStatus);
    }
}
