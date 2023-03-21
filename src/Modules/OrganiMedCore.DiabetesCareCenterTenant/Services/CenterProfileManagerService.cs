using IntelliMed.Core.Extensions;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterTenant.Services
{
    public class CenterProfileManagerService : ICenterProfileManagerService
    {
        private readonly ISharedDataAccessorService _sharedDataAccessorService;
        private readonly ShellSettings _shellSettings;


        public CenterProfileManagerService(
            ISharedDataAccessorService sharedDataAccessorService,
            ShellSettings shellSettings)
        {
            _sharedDataAccessorService = sharedDataAccessorService;
            _shellSettings = shellSettings;
        }


        public async Task<IEnumerable<CenterProfilePart>> GetCenterProfilesAsync()
        {
            IEnumerable<ContentItem> contentItems;
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                contentItems = await GetCenterProfileTenantServiceUsingScope(scope)
                    .GetCenterProfilesForTenantAsync(_shellSettings.Name);
            }

            return contentItems.Select(contentItem => contentItem.As<CenterProfilePart>());
        }

        public async Task<ContentItem> GetCenterProfileEditorForCurrentCenterAsync(bool shouldCreateNewVersion = false)
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                var service = GetCenterProfileTenantServiceUsingScope(scope);

                var contentItem = await service.GetCenterProfileAssignedToTenantAsync(_shellSettings.Name);
                if (contentItem.IsInRenewalProcess() || !shouldCreateNewVersion)
                {
                    return contentItem;
                }

                return await service.RequireCenterProfileContentItemInNewRenewalProcessAsync(contentItem, true);
            }
        }

        public async Task<(CenterProfileReviewState CurrentState, IShape DetailsShape)> GetCenterProfileForCurrentCenterAsync<T>(T updater)
            where T : Controller, IUpdateModel
        {
            ContentItem contentItem;
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                var contentItemDisplayManager = scope.ServiceProvider.GetRequiredService<IContentItemDisplayManager>();

                contentItem = await GetCenterProfileUsingScopeAsync(scope);
                updater.ViewData["Submitted"] = contentItem.As<CenterProfileManagerExtensionsPart>().Submitted();

                return (
                    contentItem.As<CenterProfileReviewStatesPart>().GetCurrentReviewState(),
                    await contentItemDisplayManager.BuildDisplayAsync(contentItem, updater, "Detail"));
            }
        }

        public async Task<ContentItem> GetCenterProfileForCurrentCenterAsync()
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                return await GetCenterProfileUsingScopeAsync(scope);
            }
        }

        public async Task SetCenterProfileAssignmentAsync(string contentItemId)
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                await GetCenterProfileTenantServiceUsingScope(scope)
                    .SetCenterProfileAssignmentAsync(contentItemId, _shellSettings.Name);
            }
        }

        public async Task DeleteCenterProfileAssignmentAsync(string contentItemId)
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                await GetCenterProfileTenantServiceUsingScope(scope)
                    .SetCenterProfileAssignmentAsync(contentItemId, null);
            }
        }

        public async Task<IEnumerable<CenterProfileEditorTerritorySearchViewModel>> SearchTerritoryByZipCode(int zipCode)
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                return await scope.ServiceProvider.GetRequiredService<ICenterProfileCommonService>()
                    .SearchTerritoryByZipCodeAsync(zipCode);
            }
        }

        public async Task SaveCenterProfileAsync(ICenterProfileViewModel viewModel)
        {
            viewModel.ThrowIfNull();

            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                var service = GetCenterProfileTenantServiceUsingScope(scope);
                var contentItem = await service.UpdateCenterProfileAsync(_shellSettings.Name, viewModel);

                await service.SaveCenterProfileAsync(contentItem, false);
            }
        }

        public async Task SubmitCenterProfileAsync(ContentItem contentItem)
        {
            if (contentItem.As<CenterProfileManagerExtensionsPart>().Submitted())
            {
                return;
            }

            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                await GetCenterProfileTenantServiceUsingScope(scope).SaveCenterProfileAsync(contentItem, true);
            }
        }

        public async Task<CenterProfileEquipmentsSettings> GetCenterProfileEquipmentSettingsAsync()
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                return await scope.ServiceProvider.GetRequiredService<ICenterProfileService>()
                    .GetCenterProfileEquipmentSettingsAsync();
            }
        }

        public async Task<Colleague> ExecuteColleagueActionAsync(ContentItem contentItem, CenterProfileColleagueActionViewModel viewModel)
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                return await GetCenterProfileTenantServiceUsingScope(scope)
                    .ExecuteColleagueActionAsync(contentItem, viewModel);
            }
        }

        public async Task<Colleague> InviteColleagueAsync(ContentItem contentItem, CenterProfileColleagueViewModel viewModel)
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                return await GetCenterProfileTenantServiceUsingScope(scope)
                    .InviteColleagueAsync(contentItem, viewModel);
            }
        }

        public async Task<PersonDataCompactViewModel> GetPersonDataCompactViewModelAsync(int memberRightId)
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                var service = scope.ServiceProvider.GetRequiredService<IDiabetesUserProfileService>();

                return await service.GetPersonDataCompactViewModel(memberRightId);
            }
        }


        private ICenterProfileTenantService GetCenterProfileTenantServiceUsingScope(IServiceScope scope)
            => scope.ServiceProvider.GetRequiredService<ICenterProfileTenantService>();

        private async Task<ContentItem> GetCenterProfileUsingScopeAsync(IServiceScope scope)
            => await GetCenterProfileTenantServiceUsingScope(scope)
                .GetCenterProfileAssignedToTenantAsync(_shellSettings.Name);
    }
}