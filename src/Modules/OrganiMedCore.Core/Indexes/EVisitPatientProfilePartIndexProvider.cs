using OrganiMedCore.Core.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace OrganiMedCore.Core.Indexes
{
    public class EVisitPatientProfilePartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<EVisitPatientProfilePartIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<EVisitPatientProfilePart>();
                    if (part == null) return null;

                    return new EVisitPatientProfilePartIndex()
                    {
                        PatientIdentifierType = part.PatientIdentifierType,
                        PatientIdentifierValue = part.PatientIdentifierValue,
                        FirstName = part.FirstName,
                        LastName = part.LastName
                    };
                });
        }
    }
}
