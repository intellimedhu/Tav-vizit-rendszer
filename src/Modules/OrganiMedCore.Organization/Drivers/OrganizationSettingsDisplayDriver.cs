using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Settings;
using OrganiMedCore.Organization.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Drivers
{
    public class OrganizationSettingsDisplayDriver : SectionDisplayDriver<ISite, OrganizationSettings>
    {
        public override IDisplayResult Edit(OrganizationSettings section, BuildEditorContext context)
        {
            return Initialize<OrganizationSettingsViewModel>("OrganizationSettings_Edit", model =>
            {
                model.EesztId = section.EesztId;
                model.EesztName = section.EesztName;
            }).Location("Content:2").OnGroup(GroupIds.OrganizationSettings);
        }

        public override async Task<IDisplayResult> UpdateAsync(OrganizationSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupIds.OrganizationSettings)
            {
                var model = new OrganizationSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                section.EesztId = model.EesztId;
                section.EesztName = model.EesztName;
            }

            return await EditAsync(section, context);
        }
    }
}
