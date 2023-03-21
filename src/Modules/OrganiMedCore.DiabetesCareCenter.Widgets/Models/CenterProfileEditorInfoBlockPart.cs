using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Models
{
    public class CenterProfileEditorInfoBlockPart : ContentPart
    {
        public CenterProfileEditorStep CenterProfileEditorStep { get; set; }

        public DiabetesCareCenterTenants DiabetesCareCenterTenants { get; set; }
    }
}
