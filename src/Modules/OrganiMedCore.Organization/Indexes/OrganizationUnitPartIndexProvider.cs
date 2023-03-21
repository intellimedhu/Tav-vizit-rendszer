using OrchardCore.ContentManagement;
using OrganiMedCore.Organization.Models;
using YesSql.Indexes;

namespace OrganiMedCore.Organization.Indexes
{
    public class OrganizationUnitPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<OrganizationUnitPartIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<OrganizationUnitPart>();
                    if (part == null) return null;

                    return new OrganizationUnitPartIndex
                    {
                        EesztId = part.EesztId,
                        EesztName = part.EesztName,
                        OrganizationUnitType = part.OrganizationUnitType
                    };
                });
        }
    }
}
