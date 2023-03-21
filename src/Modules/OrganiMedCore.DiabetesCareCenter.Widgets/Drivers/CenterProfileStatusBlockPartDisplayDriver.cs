using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class CenterProfileStatusBlockPartDisplayDriver : ContentPartDisplayDriver<CenterProfileStatusBlockPart>
    {
        public override IDisplayResult Display(CenterProfileStatusBlockPart part)
            => Dynamic("CenterProfileStatusBlockPart")
            .Location("Content:6");
    }
}
