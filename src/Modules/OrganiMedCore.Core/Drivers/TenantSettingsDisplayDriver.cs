using OrganiMedCore.Core.Constants;
using OrganiMedCore.Core.Settings;
using OrganiMedCore.Core.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrganiMedCore.Core.Drivers
{
    public class TenantSettingsDisplayDriver : SectionDisplayDriver<ISite, TenantSettings>
    {
        public override IDisplayResult Edit(TenantSettings section)
            => Initialize<TenantSettingsViewModel>("TenantSettings_Edit", viewModel => viewModel.Map(section))
            .Location("Content:2")
            .OnGroup(GroupIds.TenantSettings);

        public async override Task<IDisplayResult> UpdateAsync(TenantSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupIds.TenantSettings)
            {
                var viewModel = new TenantSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

                section.IsOrganization = viewModel.IsOrganization;
            }

            return await EditAsync(section, context);
        }
    }
}
