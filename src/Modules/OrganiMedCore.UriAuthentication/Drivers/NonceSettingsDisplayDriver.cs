using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrganiMedCore.UriAuthentication.Constants;
using OrganiMedCore.UriAuthentication.Settings;
using OrganiMedCore.UriAuthentication.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.UriAuthentication.Drivers
{
    public class NonceSettingsDisplayDriver : SectionDisplayDriver<ISite, NonceSettings>
    {
        public override IDisplayResult Edit(NonceSettings section)
        {
            return Initialize<NonceSettingsViewModel>("NonceSettings_Edit", viewModel =>
            {
                viewModel.NonceExpirationInDays = section.NonceExpirationInDays;
            })
            .Location("Content:1")
            .OnGroup(GroupIds.NonceSettings);
        }

        public override async Task<IDisplayResult> UpdateAsync(NonceSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupIds.NonceSettings)
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);
            }

            return Edit(section, context);
        }
    }
}
