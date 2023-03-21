using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrganiMedCore.Organization.Constants;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Models;
using System;

namespace OrganiMedCore.Organization.Migrations
{
    public class CheckInMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public CheckInMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }


        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(CheckInPart), builder => builder
                .WithDescription("The check in part."));

            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.CheckIn, builder => builder
                .WithPart(nameof(CheckInPart))
                .WithPart(nameof(MetaDataPart))
                .Listable()
                .Securable()
                .Versionable());

            const string Index = nameof(CheckInPartIndex);
            const string CheckInDateUtc = nameof(CheckInPartIndex.CheckInDateUtc);
            const string CheckInStatus = nameof(CheckInPartIndex.CheckInStatus);

            SchemaBuilder.CreateMapIndexTable(Index, table => table
                 .Column<DateTime>(CheckInDateUtc)
                 .Column<string>(CheckInStatus)
             );

            SchemaBuilder.AlterTable(Index, table =>
            {
                var baseName = $"IDX_{Index}_";
                table.CreateIndex(baseName + CheckInDateUtc, CheckInDateUtc);
                table.CreateIndex(baseName + CheckInStatus, CheckInStatus);
            });

            return 1;
        }
    }
}
