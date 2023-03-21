using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenterTenant.Constants;
using OrganiMedCore.DiabetesCareCenterTenant.Settings;

namespace OrganiMedCore.DiabetesCareCenterTenant.Drivers
{
    public class CenterSettingsButtonsDisplayDriver : SectionDisplayDriver<ISite, CenterSettings>
    {
        public override IDisplayResult Edit(CenterSettings section, BuildEditorContext context)
            => Dynamic("CenterSettingsButtons_Edit", model =>
            {
                model.Assigned = !string.IsNullOrEmpty(section.CenterProfileContentItemId);
                model.LeaderNotified = section.LeaderNotified;
            })
            .Location("Actions")
            .OnGroup(GroupIds.CenterSettings);
    }
}
