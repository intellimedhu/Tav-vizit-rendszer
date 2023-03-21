using OrchardCore.ContentManagement;
using OrganiMedCore.Core.Models.Enums;

namespace OrganiMedCore.Core.Models
{
    public class EVisitOrganizationUserProfilePart : ContentPart
    {
        public string StampNumber { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string EesztId { get; set; }

        public string EesztName { get; set; }

        public int SharedUserId { get; set; }

        public string HighestRankOrEducation { get; set; }

        public string MedicalExams { get; set; }

        public string QualificationLicenceExams { get; set; }

        public string QualificationNameNumberDate { get; set; }

        public OrganizationUserProfileTypes? OrganizationUserProfileType { get; set; }
    }
}
