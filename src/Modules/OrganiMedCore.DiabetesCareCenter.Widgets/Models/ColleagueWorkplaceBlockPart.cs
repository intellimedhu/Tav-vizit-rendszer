using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Models
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class ColleagueWorkplaceBlockPart : ContentPart
    {
        public ColleagueWorkplaceTab ColleagueWorkplaceTab { get; set; }

        public string ColleagueStatusGroup { get; set; }
    }
}
