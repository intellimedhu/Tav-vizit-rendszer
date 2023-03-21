using IntelliMed.DokiNetIntegration.Constants;
using IntelliMed.DokiNetIntegration.Settings;
using IntelliMed.DokiNetIntegration.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration.Drivers
{
    public class DokiNetSettingsDisplayDriver : SectionDisplayDriver<ISite, DokiNetSettings>
    {
        public override IDisplayResult Edit(DokiNetSettings section)
            => Initialize<DokiNetSettingsViewModel>("DokiNetSettings_Edit", viewModel => viewModel.UpdateViewModel(section))
                .Location("Content:2")
                .OnGroup(GroupIds.DokiNetSettings);

        public override async Task<IDisplayResult> UpdateAsync(DokiNetSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupIds.DokiNetSettings)
            {
                var viewModel = new DokiNetSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

                viewModel.UpdateModel(section);
            }

            return await EditAsync(section, context);
        }
    }
}
