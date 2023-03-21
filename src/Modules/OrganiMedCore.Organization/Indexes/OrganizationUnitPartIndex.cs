using YesSql.Indexes;

namespace OrganiMedCore.Organization.Indexes
{
    public class OrganizationUnitPartIndex : MapIndex
    {
        public string EesztId { get; set; }

        public string EesztName { get; set; }

        public string OrganizationUnitType { get; set; }
    }
}
