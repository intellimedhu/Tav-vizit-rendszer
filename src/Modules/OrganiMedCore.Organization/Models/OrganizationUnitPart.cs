using OrchardCore.ContentManagement;

namespace OrganiMedCore.Organization.Models
{
    public class OrganizationUnitPart : ContentPart
    {
        public string Name { get; set; }

        public string EesztId { get; set; }

        public string EesztName { get; set; }

        public string OrganizationUnitType { get; set; }
    }
}
