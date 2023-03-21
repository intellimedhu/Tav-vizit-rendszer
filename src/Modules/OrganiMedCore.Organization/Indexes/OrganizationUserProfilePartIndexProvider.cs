using OrchardCore.ContentManagement;
using OrganiMedCore.Organization.Models;
using YesSql.Indexes;

namespace OrganiMedCore.Organization.Indexes
{
    public class OrganizationUserProfilePartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<OrganizationUserProfilePartIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<OrganizationUserProfilePart>();
                    if (part == null) return null;

                    return new OrganizationUserProfilePartIndex
                    {
                        EVisitOrganizationUserProfileId = part.EVisitOrganizationUserProfileId
                    };
                });
        }
    }
}