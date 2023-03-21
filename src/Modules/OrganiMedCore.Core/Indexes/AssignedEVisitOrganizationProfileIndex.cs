using YesSql.Indexes;

namespace OrganiMedCore.Core.Indexes
{
    public class AssignedEVisitOrganizationProfileIndex : MapIndex
    {
        public string EVisitOrganizationUserProfileId { get; set; }
    }
}
