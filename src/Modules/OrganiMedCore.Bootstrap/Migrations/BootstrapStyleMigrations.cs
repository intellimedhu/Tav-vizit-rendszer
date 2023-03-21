using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.Bootstrap.Models;

namespace OrganiMedCore.Bootstrap.Migrations
{
    public class BootstrapStyleMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public BootstrapStyleMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(BootstrapStylePart), builder => builder
                .Attachable()
                .WithDescription("Provides a bootstrap style for your content item.")
            );

            return 1;
        }
    }
}
