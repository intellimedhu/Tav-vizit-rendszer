using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenterManager.Constants;
using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Drivers
{
    public class CenterSettingsDisplayDriver : SectionDisplayDriver<ISite, CenterManagerSettings>
    {
        private readonly ICenterProfileService _centerProfileService;
        private readonly ITerritoryService _territoryService;


        public CenterSettingsDisplayDriver(ICenterProfileService centerProfileService, ITerritoryService territoryService)
        {
            _centerProfileService = centerProfileService;
            _territoryService = territoryService;
        }


        public override IDisplayResult Edit(CenterManagerSettings section)
            => Initialize<CenterSettingsViewModel>("CenterSettings_Edit", viewModel => viewModel.UpdateViewModel(section))
            .Location("Content:2")
            .OnGroup(GroupIds.CenterProfileSettings);

        public async override Task<IDisplayResult> UpdateAsync(CenterManagerSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupIds.CenterProfileSettings)
            {
                var viewModel = new CenterSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

                var emptyCenterProfileCache =
                    viewModel.ForceEmptyCache ||
                    (!section.CenterProfileCacheEnabled && viewModel.CenterProfileCacheEnabled);

                var emptyTerritoryCache =
                    viewModel.ForceEmptyCache ||
                    (!section.TerritoryCacheEnabled && viewModel.TerritoryCacheEnabled);

                viewModel.UpdateModel(section);

                if (emptyCenterProfileCache)
                {
                    _centerProfileService.ClearCenterProfileCache();
                }

                if (emptyTerritoryCache)
                {
                    _territoryService.ClearTerritoryCache();
                }
            }

            return await EditAsync(section, context);
        }
    }
}
