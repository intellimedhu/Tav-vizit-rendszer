using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    /// <summary>
    /// This part is responsible for tracking the statuses during the renewal process.
    /// </summary>
    public class CenterProfileManagerExtensionsPart : ContentPart
    {
        public string AssignedTenantName { get; set; }

        public AccreditationStatus? RenewalAccreditationStatus { get; set; }

        public CenterProfileStatus? RenewalCenterProfileStatus { get; set; }

        public AccreditationStatusResult AccreditationStatusResult { get; set; }
    }
}
