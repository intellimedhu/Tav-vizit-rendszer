using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    public class AccreditationStatusBlockPartDisplayDriver : ContentPartDisplayDriver<AccreditationStatusBlockPart>
    {
        public override IDisplayResult Display(AccreditationStatusBlockPart part)
            => Dynamic("AccreditationStatusBlockPart")
            .Location("Content:5");
    }
}
