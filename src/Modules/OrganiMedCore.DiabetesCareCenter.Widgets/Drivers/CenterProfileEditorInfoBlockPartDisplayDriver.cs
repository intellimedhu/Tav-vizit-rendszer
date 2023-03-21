using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    public class CenterProfileEditorInfoBlockPartDisplayDriver : ContentPartDisplayDriver<CenterProfileEditorInfoBlockPart>
    {
        public override IDisplayResult Edit(CenterProfileEditorInfoBlockPart part)
        {
            return Initialize<CenterProfileEditorInfoBlockPartViewModel>(
                "CenterProfileEditorInfoBlock_Edit",
                viewModel => viewModel.UpdateViewModel(part));
        }

        public override async Task<IDisplayResult> UpdateAsync(CenterProfileEditorInfoBlockPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix);

            return Edit(part);
        }
    }
}
