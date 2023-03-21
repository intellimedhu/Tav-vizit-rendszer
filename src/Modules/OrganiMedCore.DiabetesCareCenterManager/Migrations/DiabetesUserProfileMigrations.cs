using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations
{
    public class DiabetesUserProfileMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public DiabetesUserProfileMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(DiabetesUserProfilePart), builder => builder
                .WithDescription("The Diabetes User Profile part")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.DiabetesUserProfile, builder => builder
                .WithPart(nameof(DiabetesUserProfilePart))
                .Listable()
                .Securable()
                .Versionable()
            );

            DiabetesUserProfileSchemaBuilder.Build(SchemaBuilder);

            return 1;
        }
    }
}
