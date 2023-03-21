using YesSql.Indexes;

namespace OrganiMedCore.Core.Indexes
{
    public class EVisitOrganizationUserProfilePartIndex : MapIndex
    {
        public string StampNumber { get; set; }

        public string Email { get; set; }

        public int SharedUserId { get; set; }
    }
}
