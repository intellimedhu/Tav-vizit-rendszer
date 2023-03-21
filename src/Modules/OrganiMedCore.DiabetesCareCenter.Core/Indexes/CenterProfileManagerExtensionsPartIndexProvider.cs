using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using YesSql.Indexes;

namespace OrganiMedCore.DiabetesCareCenter.Core.Indexes
{
    public class CenterProfileManagerExtensionsPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<CenterProfileManagerExtensionsPartIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<CenterProfileManagerExtensionsPart>();
                    if (part == null)
                    {
                        return null;
                    }

                    return new CenterProfileManagerExtensionsPartIndex()
                    {
                        AssignedTenantName = part.AssignedTenantName,
                        RenewalCenterProfileStatus = part.RenewalCenterProfileStatus
                    };
                });
        }
    }
}
