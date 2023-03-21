using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenter.Core.Indexes
{
    public class CenterProfilePartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
            => context.For<CenterProfilePartIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<CenterProfilePart>();
                    if (part == null)
                    {
                        return null;
                    }

                    return new CenterProfilePartIndex()
                    {
                        AccreditationStatus = part.AccreditationStatus,
                        AccreditationStatusDateUtc = part.AccreditationStatusDateUtc,
                        CenterZipCode = part.CenterZipCode,
                        MemberRightId = part.MemberRightId,
                        Created = part.Created
                    };
                });
    }
}
