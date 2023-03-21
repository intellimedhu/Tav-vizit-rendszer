using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Models;

namespace OrganiMedCore.Organization.Migrations
{
    public class PatientProfileMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public PatientProfileMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(PatientProfilePart), builder => builder
                .WithDescription("The patient profile part."));

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.PatientProfile, builder => builder
                .WithPart(nameof(PatientProfilePart))
                .Listable()
                .Securable()
                .Versionable());

            const string Index = nameof(PatientProfilePartIndex);
            const string EVisitPatientProfileId = nameof(PatientProfilePartIndex.EVisitPatientProfileId);

            SchemaBuilder.CreateMapIndexTable(Index, table => table
                .Column<string>(EVisitPatientProfileId));

            SchemaBuilder.AlterTable(Index, table =>
            {
                var indexPrefix = $"IDX_{Index}_";
                table.CreateIndex(indexPrefix + EVisitPatientProfileId, EVisitPatientProfileId);
            });

            return 1;
        }
    }
}
