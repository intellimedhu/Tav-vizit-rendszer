using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using YesSql.Sql;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema
{
    public static class CenterProfileManagerExtensionSchemaBuilder
    {
        public static void Build(ISchemaBuilder schemaBuilder)
        {
            var tableName = nameof(CenterProfileManagerExtensionsPartIndex);
            var columnAssignedTenantName = nameof(CenterProfileManagerExtensionsPartIndex.AssignedTenantName);
            var columnRenewalCenterProfileStatus = nameof(CenterProfileManagerExtensionsPartIndex.RenewalCenterProfileStatus);

            schemaBuilder.CreateMapIndexTable(tableName, table => table
                .Column<string>(columnAssignedTenantName)
                .Column<int>(columnRenewalCenterProfileStatus)
            );

            schemaBuilder.CreateColumnIndexes(tableName, columnAssignedTenantName, columnRenewalCenterProfileStatus);
        }
    }
}
