using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using YesSql.Sql;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema
{
    public static class DiabetesUserProfileSchemaBuilder
    {
        public static void Build(ISchemaBuilder schemaBuilder)
        {
            schemaBuilder.CreateMapIndexTable(nameof(DiabetesUserProfilePartIndex), table => table
                .Column<int>(nameof(DiabetesUserProfilePartIndex.MemberRightId))
            );

            schemaBuilder.CreateColumnIndexes(
                nameof(DiabetesUserProfilePartIndex),
                nameof(DiabetesUserProfilePartIndex.MemberRightId));
        }
    }
}
