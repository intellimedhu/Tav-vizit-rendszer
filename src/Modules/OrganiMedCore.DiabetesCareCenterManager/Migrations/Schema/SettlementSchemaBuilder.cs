using OrganiMedCore.DiabetesCareCenterManager.Constants;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using IntelliMed.Core.Extensions;
using YesSql.Sql;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema
{
    public static class SettlementSchemaBuilder
    {
        public static void Build(ISchemaBuilder schemaBuilder)
        {
            var tableName = nameof(SettlementIndex);
            var columnZipCode = nameof(SettlementIndex.ZipCode);
            var columnName = nameof(SettlementIndex.Name);
            var columnTerritoryId = nameof(SettlementIndex.TerritoryId);

            schemaBuilder.CreateMapIndexTable(tableName, table => table
                .Column<string>(columnZipCode, column => column.WithLength(DataFieldsProperties.SettlementZipCodeMaxLength))
                .Column<string>(columnName, column => column.WithLength(DataFieldsProperties.SettlementNameMaxLength))
                .Column<int>(columnTerritoryId)
            );

            schemaBuilder.CreateColumnIndexes(tableName, columnZipCode, columnName, columnTerritoryId);
        }
    }
}
