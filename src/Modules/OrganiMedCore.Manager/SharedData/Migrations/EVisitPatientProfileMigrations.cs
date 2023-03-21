using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.Core.Constants;
using OrganiMedCore.Core.Indexes;
using OrganiMedCore.Core.Models;

namespace OrganiMedCore.Manager.SharedData.Migrations
{
    public class EVisitPatientProfileMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public EVisitPatientProfileMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(EVisitPatientProfilePart), builder => builder
                .WithDescription("The shared patient profile part."));

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.EVisitPatientProfile, builder => builder
                .WithPart(nameof(EVisitPatientProfilePart))
                .Versionable());

            const string Index = nameof(EVisitPatientProfilePartIndex);
            const string PatientIdentifierType = nameof(EVisitPatientProfilePartIndex.PatientIdentifierType);
            const string PatientIdentifierValue = nameof(EVisitPatientProfilePartIndex.PatientIdentifierValue);
            const string FirstName = nameof(EVisitPatientProfilePartIndex.FirstName);
            const string LastName = nameof(EVisitPatientProfilePartIndex.LastName);

            SchemaBuilder.CreateMapIndexTable(Index, table => table
                .Column<string>(PatientIdentifierType)
                .Column<string>(PatientIdentifierValue)
                .Column<string>(FirstName)
                .Column<string>(LastName));

            SchemaBuilder.AlterTable(Index, table =>
                {
                    var indexPrefix = $"IDX_{Index}_";
                    table.CreateIndex(indexPrefix + PatientIdentifierType, PatientIdentifierType);
                    table.CreateIndex(indexPrefix + PatientIdentifierValue, PatientIdentifierValue);
                    table.CreateIndex(indexPrefix + FirstName, FirstName);
                    table.CreateIndex(indexPrefix + LastName, LastName);
                });

            return 1;
        }
    }
}
