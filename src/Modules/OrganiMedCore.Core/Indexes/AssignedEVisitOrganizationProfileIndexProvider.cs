using OrganiMedCore.Core.Models;
using YesSql.Indexes;

namespace OrganiMedCore.Core.Indexes
{
    public class AssignedEVisitOrganizationProfileIndexProvider : IndexProvider<AssignedEVisitOrganizationProfile>
    {
        public override void Describe(DescribeContext<AssignedEVisitOrganizationProfile> context)
        {
            context.For<AssignedEVisitOrganizationProfileIndex>()
                .Map(item =>
                {
                    return new AssignedEVisitOrganizationProfileIndex
                    {
                        EVisitOrganizationUserProfileId = item.EVisitOrganizationUserProfileId
                    };
                });
        }
    }
}