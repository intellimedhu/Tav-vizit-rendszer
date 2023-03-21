using YesSql.Indexes;

namespace OrganiMedCore.Organization.Indexes
{
    public class MetaDataPartIndex : MapIndex
    {
        public string OrganizationUnitId { get; set; }

        public string EVisitPatientProfileId { get; set; }

        public string EVisitOrganizationUserProfileId { get; set; }
    }
}