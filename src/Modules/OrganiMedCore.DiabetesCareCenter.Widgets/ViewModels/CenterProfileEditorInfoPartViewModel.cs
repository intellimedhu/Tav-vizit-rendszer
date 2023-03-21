using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels
{
    // Referenced in CenterProfileEditorInfo.liquid
    public class CenterProfileEditorInfoPartViewModel
    {
        public ContentItem ContentItem { get; set; }

        public DiabetesCareCenterTenants DiabetesCareCenterTenants { get; set; }
    }
}
