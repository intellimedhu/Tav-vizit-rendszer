using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations
{
    public class CenterProfileMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public CenterProfileMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfilePart), builder => builder
                .WithDescription("The center profile part.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfile, builder => builder
                .WithPart(nameof(CenterProfilePart))
                .Listable()
                .Securable()
                .Versionable()
            );

            CenterProfileSchemaBuilder.Build(SchemaBuilder);

            return 1;
        }
    }
}
