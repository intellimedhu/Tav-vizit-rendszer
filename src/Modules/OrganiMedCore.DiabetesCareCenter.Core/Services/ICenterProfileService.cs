using IntelliMed.Core.Exceptions;
using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface ICenterProfileService
    {
        Task<bool> CacheEnabledAsync();

        Task<IEnumerable<ContentItem>> GetCenterProfilesAsync();

        /// <exception cref="UnauthorizedException"></exception>
        Task<ContentItem> GetPermittedCenterProfileAsync(string id);

        Task<IEnumerable<ContentItem>> GetPermittedCenterProfilesAsync();

        Task DeleteCenterProfileAsync(string contentItemId);

        Task DeleteOwnCenterProfileAsync(string contentItemId, int memberRightId);

        Task<ContentItem> GetCenterProfileAsync(string contentItemId);

        Task<ContentItem> GetCenterProfileToLeaderAsync(int memberRightId, string contentItemId);

        Task<IEnumerable<ContentItem>> GetCenterProfilesToLeaderAsync(int memberRightId);

        Task<IEnumerable<ContentItem>> GetCenterProfileVersionsAsync(string contentItemId);

        Task ReviewAsync(CenterProfileReviewCheckResult reviewCheckResult, bool accepted, string comment);

        /// <exception cref="ArgumentException">If the collection is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If any of the the collection's item has an invalid <see cref="AccreditationStatus"/>. Allowed statuses are: null, A, TA, R.</exception>
        /// <exception cref="ArgumentNullException">Only theoretical possibility.</exception>
        Task<IEnumerable<string>> AcceptManyAsync(IEnumerable<CenterProfileDecisionStateViewModel> viewModelStates);

        Task<CenterProfileEquipmentsSettings> GetCenterProfileEquipmentSettingsAsync();

        Task SetCenterProfileEquipmentSettingsAsync(CenterProfileEquipmentsSettings settings);

        Task SaveCenterProfileAsync(ContentItem contentItem, ICenterProfileViewModel viewModel);

        Task<ContentItem> NewCenterProfileAsync(int memberRightId, bool shouldCreate);

        Task SaveCenterProfileAsync(ContentItem contentItem, bool submitted);

        Task CalculateAccreditationStatusAsync(ContentItem contentItem, Action<CenterProfileManagerExtensionsPart> alteration = null);

        Task<IEnumerable<ContentItem>> GetUnsubmittedCenterProfilesAsync(DateTime renewalStartDate);

        Task ChangeCenterProfileLeaderAsync(ContentItem contentItem, int memberRightId);

        Task OnUserProfilesUpdatedAsync(IEnumerable<int> memberRightIds);

        Task<IEnumerable<ContentItem>> LoadCenterProfilesFromCacheAsync();

        void ClearCenterProfileCache();
    }
}
