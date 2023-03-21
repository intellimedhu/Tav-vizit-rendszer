using IntelliMed.Core.Extensions;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Models;

namespace OrganiMedCore.Organization.Migrations
{
    public class OrganizationUnitMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public OrganizationUnitMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(OrganizationUnitPart), builder => builder
                .WithDescription("The organization unit part."));

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.OrganizationUnit, builder => builder
                .WithPart(nameof(OrganizationUnitPart))
                .Creatable()
                .Listable()
                .Securable()
                .Versionable());

            var tableName = nameof(OrganizationUnitPartIndex);
            var columnEesztId = nameof(OrganizationUnitPartIndex.EesztId);
            var columnEesztName = nameof(OrganizationUnitPartIndex.EesztName);
            var columnOrganizationUnitType = nameof(OrganizationUnitPartIndex.OrganizationUnitType);

            SchemaBuilder.CreateMapIndexTable(tableName, table => table
                 .Column<string>(columnEesztId)
                 .Column<string>(columnEesztName)
                 .Column<string>(columnOrganizationUnitType));

            SchemaBuilder.CreateColumnIndexes(tableName, columnEesztId, columnEesztName, columnOrganizationUnitType);

            return 1;
        }
    }
}
