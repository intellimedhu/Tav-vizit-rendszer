using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.Testing.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests.MockServices
{
    public class CenterProfileService_RenewalPeriodService : ICenterProfileService
    {
        private readonly bool _shouldReturnEmpty;
        private readonly ISession _session;


        public CenterProfileService_RenewalPeriodService(bool shouldReturnEmpty, ISession session)
        {
            _shouldReturnEmpty = shouldReturnEmpty;
            _session = session;
        }


        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<string>> AcceptManyAsync(IEnumerable<CenterProfileDecisionStateViewModel> viewModelStates)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<bool> CacheEnabledAsync()
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task CalculateAccreditationStatusAsync(ContentItem contentItem, Action<CenterProfileManagerExtensionsPart> alteration = null)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task ChangeCenterProfileLeaderAsync(ContentItem contentItem, int memberRightId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public void ClearCenterProfileCache()
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public void ClearCenterProfilesCache()
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task DeleteCenterProfileAsync(string contentItemId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task DeleteOwnCenterProfileAsync(string contentItemId, int memberRightId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<ContentItem> GetCenterProfileAsync(string contentItemId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<CenterProfileEquipmentsSettings> GetCenterProfileEquipmentSettingsAsync()
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<ContentItem>> GetCenterProfilesAsync()
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<ContentItem>> GetCenterProfilesToLeaderAsync(int memberRightId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<ContentItem> GetCenterProfileToLeaderAsync(int memberRightId, string contentItemId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<ContentItem>> GetCenterProfileVersionsAsync(string contentItemId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<ContentItem> GetPermittedCenterProfileAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ContentItem>> GetPermittedCenterProfilesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ContentItem>> GetUnsubmittedCenterProfilesAsync(DateTime renewalStartDate)
        {
            if (_shouldReturnEmpty)
            {
                return Enumerable.Empty<ContentItem>();
            }

            var cm = new ContentManagerMock(_session);
            var result = new List<ContentItem>();
            for (var i = 1; i <= 5; i++)
            {
                var cp1 = await cm.NewAsync(ContentTypes.CenterProfile);
                cp1.Alter<CenterProfilePart>(part =>
                {
                    // Mock DokiNetService should contain this value:
                    part.MemberRightId = i * 100;

                    part.CenterName = $"A{part.MemberRightId}";
                    part.Created = true;
                });
                cp1.Alter<CenterProfileManagerExtensionsPart>(part =>
                {
                    part.AssignedTenantName = $"DCC{i}";
                });

                await cm.CreateAsync(cp1);

                result.Add(cp1);
            }

            return result;
        }

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<ContentItem>> LoadCenterProfilesFromCacheAsync()
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<ContentItem> NewCenterProfileAsync(int memberRightId, bool shouldCreate)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task OnUserProfilesUpdatedAsync(IEnumerable<int> memberRightIds)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task ReviewAsync(CenterProfileReviewCheckResult reviewCheckResult, bool accepted, string comment)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task SaveCenterProfileAsync(ContentItem contentItem, ICenterProfileViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task SaveCenterProfileAsync(ContentItem contentItem, bool submitted)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task SetCenterProfileEquipmentSettingsAsync(CenterProfileEquipmentsSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
