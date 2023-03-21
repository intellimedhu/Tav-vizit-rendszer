using OrchardCore.ContentManagement;
using OrganiMedCore.Organization.Models;
using YesSql.Indexes;

namespace OrganiMedCore.Organization.Indexes
{
    public class MetaDataPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<MetaDataPartIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<MetaDataPart>();
                    if (part == null) return null;

                    return new MetaDataPartIndex
                    {
                        OrganizationUnitId = part.OrganizationUnitId,
                        EVisitPatientProfileId = part.EVisitPatientProfileId
                    };
                });
        }
    }
}
