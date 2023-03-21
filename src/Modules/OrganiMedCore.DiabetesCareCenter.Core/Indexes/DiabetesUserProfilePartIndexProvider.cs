using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenter.Core.Indexes
{
    public class DiabetesUserProfilePartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<DiabetesUserProfilePartIndex>()
                .Map(contentItem =>
                {
                    if (contentItem.ContentType != ContentTypes.DiabetesUserProfile)
                    {
                        return null;
                    }

                    return new DiabetesUserProfilePartIndex()
                    {
                        MemberRightId = contentItem.As<DiabetesUserProfilePart>().MemberRightId
                    };
                });
        }
    }
}
