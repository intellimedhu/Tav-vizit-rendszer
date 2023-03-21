using IntelliMed.Core.Extensions;
using IntelliMed.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels
{
    public class ColleagueWorkplaceBlockPartViewModel : IContentPartViewModel<ColleagueWorkplaceBlockPart>
    {
        public ColleagueWorkplaceTab ColleagueWorkplaceTab { get; set; }

        public string ColleagueStatusGroup { get; set; }


        public void UpdatePart(ColleagueWorkplaceBlockPart part)
        {
        }

        public void UpdateViewModel(ColleagueWorkplaceBlockPart part)
        {
            part.ThrowIfNull();

            ColleagueWorkplaceTab = part.ColleagueWorkplaceTab;
            ColleagueStatusGroup = part.ColleagueStatusGroup;
        }
    }
}
