using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Widgets.Services;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.Organization.DiabetesCareCenter.Widgets")]
    public class CenterProfileEditorInfoPartOrganizationDisplayDriver : CenterProfileEditorInfoPartDisplayDriver
    {
        protected override DiabetesCareCenterTenants DiabetesCareCenterTenants => DiabetesCareCenterTenants.Organization;


        public CenterProfileEditorInfoPartOrganizationDisplayDriver(ICenterProfileInfoService centerProfileEditorInfoService)
            : base(centerProfileEditorInfoService)
        {
        }
    }
}
