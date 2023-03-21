using YesSql.Indexes;

namespace OrganiMedCore.Organization.Indexes
{
    public class PatientProfilePartIndex : MapIndex
    {
        public string EVisitPatientProfileId { get; set; }
    }
}
