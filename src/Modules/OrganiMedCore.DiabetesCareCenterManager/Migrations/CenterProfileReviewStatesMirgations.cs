using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations
{
    public class CenterProfileReviewStatesMirgations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        

        public CenterProfileReviewStatesMirgations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileReviewStatesPart), builder => builder
                .WithDescription("The center profile review states part.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfile, builder => builder
                .WithPart(nameof(CenterProfileReviewStatesPart))
            );

            return 1;
        }
    }
}
