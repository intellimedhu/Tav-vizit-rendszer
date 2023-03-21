using IntelliMed.Core.Extensions;
using IntelliMed.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels
{
    public class CenterProfileEditorInfoBlockPartViewModel : IContentPartViewModel<CenterProfileEditorInfoBlockPart>
    {
        public CenterProfileEditorStep CenterProfileEditorStep { get; set; }

        public DiabetesCareCenterTenants DiabetesCareCenterTenants { get; set; }


        public void UpdatePart(CenterProfileEditorInfoBlockPart part)
        {
            part.ThrowIfNull();

            part.CenterProfileEditorStep = CenterProfileEditorStep;
            part.DiabetesCareCenterTenants = DiabetesCareCenterTenants;
        }

        public void UpdateViewModel(CenterProfileEditorInfoBlockPart part)
        {
            part.ThrowIfNull();

            CenterProfileEditorStep = part.CenterProfileEditorStep;
            DiabetesCareCenterTenants = part.DiabetesCareCenterTenants;
        }
    }
}
