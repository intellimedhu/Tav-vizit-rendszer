using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenterManager.Constants;
using OrganiMedCore.DiabetesCareCenterManager.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Drivers
{
    public class PersonalConditionSettingsDisplayDriver : SectionDisplayDriver<ISite, AccreditationPersonalConditionSettings>
    {
        public override IDisplayResult Edit(AccreditationPersonalConditionSettings section)
            => Initialize<PersonalConditionSettingsViewModel>("PersonalConditionSettings_Edit", viewModel => viewModel.UpdateViewModel(section))
            .Location("Content:2")
            .OnGroup(GroupIds.PersonalConditionSettings);

        public override Task<IDisplayResult> UpdateAsync(AccreditationPersonalConditionSettings section, BuildEditorContext context)
        {
            if (context.GroupId == GroupIds.PersonalConditionSettings)
            {
                var viewModel = new PersonalConditionSettingsViewModel();
                context.Updater.TryUpdateModelAsync(viewModel);

                viewModel.UpdateModel(section);
            }

            return Task.FromResult(Edit(section));
        }
    }
}
