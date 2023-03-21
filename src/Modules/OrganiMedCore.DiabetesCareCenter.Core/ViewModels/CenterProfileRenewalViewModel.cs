using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileRenewalViewModel
    {
        public string AssignedTenantName { get; set; }

        [JsonProperty("renewalAccreditationStatus")]
        public AccreditationStatus? RenewalAccreditationStatus { get; set; }

        [JsonProperty("renewalCenterProfileStatus")]
        public CenterProfileStatus? RenewalCenterProfileStatus { get; set; }


        public void UpdateViewModel(CenterProfileManagerExtensionsPart part)
        {
            AssignedTenantName = part.AssignedTenantName;
            RenewalAccreditationStatus = part.RenewalAccreditationStatus;
            RenewalCenterProfileStatus = part.RenewalCenterProfileStatus;
        }
    }
}
