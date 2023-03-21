using OrchardCore.ContentManagement;
using OrganiMedCore.Organization.Models;
using YesSql.Indexes;

namespace OrganiMedCore.Organization.Indexes
{
    public class CheckInPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<CheckInPartIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<CheckInPart>();
                    if (part == null) return null;

                    return new CheckInPartIndex
                    {
                        CheckInDateUtc = part.CheckInDateUtc,
                        CheckInStatus = part.CheckInStatus
                    };
                });
        }
    }
}
