using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Models;

namespace OrganiMedCore.Organization.Migrations
{
    public class MetaDataPartMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public MetaDataPartMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(MetaDataPart), builder => builder
                .WithDescription("The meta data part.")
                .Attachable());

            const string Index = nameof(MetaDataPartIndex);
            const string OrganizationUnitId = nameof(MetaDataPartIndex.OrganizationUnitId);
            const string EVisitPatientProfileId = nameof(MetaDataPartIndex.EVisitPatientProfileId);
            const string EVisitOrganizationUserProfileId = nameof(MetaDataPartIndex.EVisitOrganizationUserProfileId);

            SchemaBuilder.CreateMapIndexTable(Index, table => table
                 .Column<string>(OrganizationUnitId)
                 .Column<string>(EVisitPatientProfileId)
                 .Column<string>(EVisitOrganizationUserProfileId)
             );

            SchemaBuilder.AlterTable(Index, table =>
                {
                    var baseName = $"IDX_{Index}_";
                    table.CreateIndex(baseName + OrganizationUnitId, OrganizationUnitId);
                    table.CreateIndex(baseName + EVisitPatientProfileId, EVisitPatientProfileId);
                    table.CreateIndex(baseName + EVisitOrganizationUserProfileId, EVisitOrganizationUserProfileId);
                });

            return 1;
        }
    }
}
