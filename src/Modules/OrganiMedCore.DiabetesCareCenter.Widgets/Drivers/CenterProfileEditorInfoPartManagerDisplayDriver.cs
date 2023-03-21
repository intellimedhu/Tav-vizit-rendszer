using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Widgets.Services;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class CenterProfileEditorInfoPartManagerDisplayDriver : CenterProfileEditorInfoPartDisplayDriver
    {
        protected override DiabetesCareCenterTenants DiabetesCareCenterTenants => DiabetesCareCenterTenants.Manager;


        public CenterProfileEditorInfoPartManagerDisplayDriver(ICenterProfileInfoService centerProfileEditorInfoService)
            : base(centerProfileEditorInfoService)
        {
        }
    }
}
