using OrchardCore.ContentManagement;
using System;

namespace OrganiMedCore.Organization.Models
{
    public class PatientProfilePart : ContentPart
    {
        public string Phone { get; set; }

        public string PhoneSecondary { get; set; }

        public string EmailSecondary { get; set; }

        public string Occupation { get; set; }

        public string PublicHealthCardNumber { get; set; }

        public DateTime? PublicHealthCardValidationDate { get; set; }

        public string Remarks { get; set; }

        public string EVisitPatientProfileId { get; set; }
    }
}
