using OrganiMedCore.DiabetesCareCenterManager.Constants;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using IntelliMed.Core.Extensions;
using YesSql.Sql;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema
{
    public static class TerritorySchemaBuilder
    {
        public static void Build(ISchemaBuilder schemaBuilder)
        {
            var indexTable = nameof(TerritoryIndex);
            var columnNameTerritorialRapporteurId = nameof(TerritoryIndex.TerritorialRapporteurId);
            var columnNameTerritoryName = nameof(TerritoryIndex.Name);

            schemaBuilder.CreateMapIndexTable(indexTable, table => table
                .Column<int>(columnNameTerritorialRapporteurId)
                .Column<string>(columnNameTerritoryName, column => column.WithLength(DataFieldsProperties.TerritoryNameMaxLength))
            );

            schemaBuilder.CreateColumnIndexes(indexTable, columnNameTerritorialRapporteurId, columnNameTerritoryName);
        }
    }
}
