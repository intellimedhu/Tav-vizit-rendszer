using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenter.Core.Indexes
{
    public class CenterProfileManagerExtensionsPartIndex : MapIndex
    {
        public string AssignedTenantName { get; set; }

        public CenterProfileStatus? RenewalCenterProfileStatus { get; set; }
    }
}
