using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations
{
    public class CenterProfileManagerExtensionsMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public CenterProfileManagerExtensionsMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(CenterProfileManagerExtensionsPart), builder => builder
                .WithDescription("The manager's center profile extensions part.")
            );

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CenterProfile, builder => builder
                .WithPart(nameof(CenterProfileManagerExtensionsPart))
            );

            CenterProfileManagerExtensionSchemaBuilder.Build(SchemaBuilder);

            return 1;
        }
    }
}
