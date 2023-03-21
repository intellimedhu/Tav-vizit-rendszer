using OrchardCore.Data.Migration;
using OrganiMedCore.Core.Indexes;

namespace OrganiMedCore.Organization.Migrations
{
    public class AssignedEVisitOrganizationProfileMigrations : DataMigration
    {
        public int Create()
        {
            const string Index = nameof(AssignedEVisitOrganizationProfileIndex);
            const string EVisitOrganizationUserProfileId = nameof(AssignedEVisitOrganizationProfileIndex.EVisitOrganizationUserProfileId);

            SchemaBuilder.CreateMapIndexTable(Index, table => table
                 .Column<string>(EVisitOrganizationUserProfileId)
             );

            SchemaBuilder.AlterTable(Index, table =>
            {
                var baseName = $"IDX_{Index}_";
                table.CreateIndex(baseName + EVisitOrganizationUserProfileId, EVisitOrganizationUserProfileId);
            });

            return 1;
        }
    }
}
