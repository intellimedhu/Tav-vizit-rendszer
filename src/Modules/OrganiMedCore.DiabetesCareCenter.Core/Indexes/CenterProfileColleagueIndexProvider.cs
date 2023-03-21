using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Linq;
using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenter.Core.Indexes
{
    public class CenterProfileColleagueIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<CenterProfileColleagueIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<CenterProfilePart>();
                    if (part == null)
                    {
                        return null;
                    }

                    return part.Colleagues.Select(colleague => new CenterProfileColleagueIndex()
                    {
                        CenterProfileContentItemId = contentItem.ContentItemId,
                        CenterProfileContentItemVersionId = contentItem.ContentItemVersionId,
                        ColleagueId = colleague.Id,
                        Email = colleague.Email,
                        FirstName = colleague.FirstName,
                        LastName = colleague.LastName,
                        MemberRightId = colleague.MemberRightId,
                        Occupation = colleague.Occupation,
                        Prefix = colleague.Prefix
                    });
                });
        }
    }
}
