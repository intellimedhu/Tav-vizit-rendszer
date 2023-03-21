using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.Entities;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace IntelliMed.DokiNetIntegration.Indexes
{
    public class DokiNetMemberIndexProvider : IndexProvider<User>
    {
        public override void Describe(DescribeContext<User> context)
            => context.For<DokiNetMemberIndex>()
                .Map(user =>
                {
                    var member = user.As<DokiNetMember>();

                    return new DokiNetMemberIndex()
                    {
                        MemberRightId = member.MemberRightId,
                        FirstName = member.FirstName,
                        LastName = member.LastName,
                        Prefix = member.Prefix,
                        UserName = member.UserName,
                        StampNumber = member.StampNumber
                    };
                });
    }
}
