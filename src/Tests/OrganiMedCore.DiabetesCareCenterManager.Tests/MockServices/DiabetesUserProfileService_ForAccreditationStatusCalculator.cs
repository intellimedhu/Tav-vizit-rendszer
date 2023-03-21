using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests.MockServices
{
    public class DiabetesUserProfileService_ForAccreditationStatusCalculator : IDiabetesUserProfileService
    {
        private readonly IEnumerable<ContentItem> _memberProfilesExpectedResult;


        public DiabetesUserProfileService_ForAccreditationStatusCalculator(
            IEnumerable<ContentItem> memberProfilesExpectedResult)
        {
            _memberProfilesExpectedResult = memberProfilesExpectedResult;
        }


        [ExcludeFromCodeCoverage]
        public Task<PersonDataCompactViewModel> GetPersonDataCompactViewModel(int memberRightId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<ContentItem> GetProfileByMemberRightIdAsync(int memberRightId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<ContentItem>> GetProfilesByMemberRightIdsAsync(IEnumerable<int> memberRightIds)
        {
            return Task.FromResult(_memberProfilesExpectedResult);
        }

        [ExcludeFromCodeCoverage]
        public Task<bool> HasMissingQualificationsForOccupation<T>(Occupation occupation, T memberRightId) where T : DokiNetMember
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<object> InitializeProfileEditorAsync(int memberRightId)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task SetPartialProfileByMemberRightIdAsync(Occupation occupation, int memberRightId, DiabetesUserProfilePartViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task SetProfileByMemberRightIdAsync(int memberRightId, DiabetesUserProfilePartViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task UpdatePartialProfileAsync(Occupation occupation, int memberRightId, DiabetesUserProfilePartViewModel viewModel, IUpdateModel updater)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task UpdatePartialProfileAsync(Occupation occupation, DokiNetMember dokiNetMember, DiabetesUserProfilePartViewModel viewModel, IUpdateModel updater)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task UpdateProfileAsync(int memberRightId, DiabetesUserProfilePartViewModel viewModel, IUpdateModel updater)
        {
            throw new NotImplementedException();
        }
    }
}
