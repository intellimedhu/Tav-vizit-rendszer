using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface ICenterProfileManagerService
    {
        Task<IEnumerable<CenterProfilePart>> GetCenterProfilesAsync();

        Task<ContentItem> GetCenterProfileEditorForCurrentCenterAsync(bool shouldCreateNewVersion = false);

        Task<(CenterProfileReviewState CurrentState, IShape DetailsShape)> GetCenterProfileForCurrentCenterAsync<T>(T updater) where T : Controller, IUpdateModel;

        Task<ContentItem> GetCenterProfileForCurrentCenterAsync();

        Task SetCenterProfileAssignmentAsync(string contentItemId);

        Task DeleteCenterProfileAssignmentAsync(string centerProfileContentItemId);

        Task<IEnumerable<CenterProfileEditorTerritorySearchViewModel>> SearchTerritoryByZipCode(int zipCode);

        Task SaveCenterProfileAsync(ICenterProfileViewModel viewModel);

        Task SubmitCenterProfileAsync(ContentItem contentItem);

        Task<CenterProfileEquipmentsSettings> GetCenterProfileEquipmentSettingsAsync();

        /// <exception cref="ColleagueNotFoundException"></exception>
        /// <exception cref="ColleagueException"></exception>
        Task<Colleague> ExecuteColleagueActionAsync(ContentItem contentItem, CenterProfileColleagueActionViewModel viewModel);

        /// <exception cref="MemberNotFoundException"></exception>
        /// <exception cref="ColleagueAlreadyExistsException"></exception>
        /// <exception cref="ColleagueEmailAlreadyTakenException"></exception>
        /// <exception cref="ColleagueException"></exception>
        Task<Colleague> InviteColleagueAsync(ContentItem contentItem, CenterProfileColleagueViewModel viewModel);

        /// <summary>
        /// Returns the member's qualifications, the related qualification objects and private phone.
        /// </summary>
        Task<PersonDataCompactViewModel> GetPersonDataCompactViewModelAsync(int memberRightId);
    }
}
