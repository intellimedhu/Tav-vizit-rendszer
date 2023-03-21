using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface IDiabetesUserProfileService
    {
        Task<ContentItem> GetProfileByMemberRightIdAsync(int memberRightId);

        Task<IEnumerable<ContentItem>> GetProfilesByMemberRightIdsAsync(IEnumerable<int> memberRightIds);

        Task SetProfileByMemberRightIdAsync(int memberRightId, DiabetesUserProfilePartViewModel viewModel);

        Task SetPartialProfileByMemberRightIdAsync(Occupation occupation, int memberRightId, DiabetesUserProfilePartViewModel viewModel);

        Task<object> InitializeProfileEditorAsync(int memberRightId);

        Task<PersonDataCompactViewModel> GetPersonDataCompactViewModel(int memberRightId);

        Task<bool> HasMissingQualificationsForOccupation<T>(Occupation occupation, T dokiNetMember) where T : DokiNetMember;

        Task UpdateProfileAsync(int memberRightId, DiabetesUserProfilePartViewModel viewModel, IUpdateModel updater);

        Task UpdatePartialProfileAsync(Occupation occupation, DokiNetMember dokiNetMember, DiabetesUserProfilePartViewModel viewModel, IUpdateModel updater);
    }
}
