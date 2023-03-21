using OrganiMedCore.Core.Models.Enums;

namespace OrganiMedCore.Core.Models
{
    public class PatientIdentifier
    {
        public string Text { get; set; }

        public PatientIdentifierTypes Value { get; set; }
    }
}
