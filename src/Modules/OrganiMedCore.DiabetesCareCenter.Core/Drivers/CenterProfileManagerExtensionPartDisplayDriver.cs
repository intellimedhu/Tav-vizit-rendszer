using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;

namespace OrganiMedCore.DiabetesCareCenter.Core.Drivers
{
    public class CenterProfileManagerExtensionPartDisplayDriver : ContentPartDisplayDriver<CenterProfileManagerExtensionsPart>
    {
        public override IDisplayResult Display(CenterProfileManagerExtensionsPart part)
            => Initialize<CenterProfileRenewalViewModel>("CenterProfileManagerExtensionsPart", viewModel => viewModel.UpdateViewModel(part))
                .Location("Detail", "Content:1");
    }
}
