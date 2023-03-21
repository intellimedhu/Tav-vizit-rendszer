using OrchardCore.ContentManagement;
using OrganiMedCore.Organization.Models;
using YesSql.Indexes;

namespace OrganiMedCore.Organization.Indexes
{
    public class PatientProfilePartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<PatientProfilePartIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<PatientProfilePart>();
                    if (part == null) return null;

                    return new PatientProfilePartIndex()
                    {
                        EVisitPatientProfileId = part.EVisitPatientProfileId
                    };
                });
        }
    }
}