using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class ColleagueWorkplaceBlockPartDisplayDriver : ContentPartDisplayDriver<ColleagueWorkplaceBlockPart>
    {
        public override IDisplayResult Edit(ColleagueWorkplaceBlockPart part)
            => Initialize<ColleagueWorkplaceBlockPartViewModel>(
                "ColleagueWorkplaceBlock_Edit",
                viewModel => viewModel.UpdateViewModel(part));

        public override async Task<IDisplayResult> UpdateAsync(ColleagueWorkplaceBlockPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix);

            return Edit(part);
        }
    }
}
