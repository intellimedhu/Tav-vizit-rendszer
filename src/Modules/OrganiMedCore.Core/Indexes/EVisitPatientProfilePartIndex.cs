using OrganiMedCore.Core.Models.Enums;
using YesSql.Indexes;

namespace OrganiMedCore.Core.Indexes
{
    public class EVisitPatientProfilePartIndex : MapIndex
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PatientIdentifierValue { get; set; }

        public PatientIdentifierTypes PatientIdentifierType { get; set; }
    }
}
