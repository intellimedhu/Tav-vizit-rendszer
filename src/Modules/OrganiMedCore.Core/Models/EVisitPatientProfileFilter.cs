using System;

namespace OrganiMedCore.Core.Models
{
    public class EVisitPatientProfileFilter
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string MothersName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string PatientIdentifierValue { get; set; }

        public bool OnlyLocalPatinets { get; set; }

        public bool IsEmpty() =>
            string.IsNullOrEmpty(FirstName) &&
            string.IsNullOrEmpty(LastName) &&
            string.IsNullOrEmpty(Email) &&
            string.IsNullOrEmpty(MothersName) &&
            !DateOfBirth.HasValue &&
            string.IsNullOrEmpty(PatientIdentifierValue);
    }
}
