using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.InfoWidgets.Models;
using OrganiMedCore.InfoWidgets.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.InfoWidgets.Drivers
{
    public class InfoBlockPartDisplayDriver : ContentPartDisplayDriver<InfoBlockPart>
    {
        public override IDisplayResult Edit(InfoBlockPart part)
        {
            return Initialize<InfoBlockViewModel>("InfoBlock_Edit", viewModel => viewModel.BlockTitle = part.BlockTitle);
        }

        public override async Task<IDisplayResult> UpdateAsync(InfoBlockPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix);

            return Edit(part);
        }
    }
}
