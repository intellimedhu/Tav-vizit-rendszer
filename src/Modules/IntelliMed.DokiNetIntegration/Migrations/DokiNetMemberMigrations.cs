using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Indexes;
using OrchardCore.Data.Migration;

namespace IntelliMed.DokiNetIntegration.Migrations
{
    public class DokiNetMemberMigrations : DataMigration
    {
        public int Create()
        {
            var tableName = nameof(DokiNetMemberIndex);
            var columnMemberRightId = nameof(DokiNetMemberIndex.MemberRightId);
            var columnFirstName = nameof(DokiNetMemberIndex.FirstName);
            var columnLastName = nameof(DokiNetMemberIndex.LastName);
            var columnPrefix = nameof(DokiNetMemberIndex.Prefix);
            var columnUserName = nameof(DokiNetMemberIndex.UserName);
            var columnStampNumber = nameof(DokiNetMemberIndex.StampNumber);

            SchemaBuilder.CreateMapIndexTable(tableName, table => table
                .Column<int>(columnMemberRightId)
                .Column<string>(columnPrefix)
                .Column<string>(columnFirstName)
                .Column<string>(columnLastName)
                .Column<string>(columnUserName)
                .Column<string>(columnStampNumber)
            );

            SchemaBuilder.CreateColumnIndexes(tableName,
                columnMemberRightId,
                columnPrefix,
                columnFirstName,
                columnLastName,
                columnUserName,
                columnStampNumber);

            return 1;
        }
    }
}
