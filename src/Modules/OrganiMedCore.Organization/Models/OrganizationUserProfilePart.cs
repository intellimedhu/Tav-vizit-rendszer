using OrchardCore.ContentManagement;
using OrganiMedCore.Core.Models.Enums;
using System.Collections.Generic;

namespace OrganiMedCore.Organization.Models
{
    public class OrganizationUserProfilePart : ContentPart
    {
        public string Phone { get; set; }

        public string Email { get; set; }

        public string OrganizationRank { get; set; }

        public string AntszLicenseNumber { get; set; }

        public string ConsultationHours { get; set; }

        public string CheckInMode { get; set; }

        public string EVisitOrganizationUserProfileId { get; set; }

        public string SignedInOrganizationUnitId { get; set; }

        public HashSet<string> PermittedOrganizationUnits { get; set; } = new HashSet<string>();

        public OrganizationUserProfileTypes? OrganizationUserProfileType { get; set; }
    }
}
