using OrchardCore.ContentManagement;

namespace OrganiMedCore.Organization.Models
{
    public class MetaDataPart : ContentPart
    {
        public string OrganizationUnitId { get; set; }

        public string EVisitPatientProfileId { get; set; }

        public string EVisitOrganizationUserProfileId { get; set; }
    }
}
