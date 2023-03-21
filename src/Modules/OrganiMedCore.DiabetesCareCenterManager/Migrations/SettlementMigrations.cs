using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrchardCore.Data.Migration;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations
{
    public class SettlementMigrations : DataMigration
    {
        public int Create()
        {
            SettlementSchemaBuilder.Build(SchemaBuilder);

            return 1;
        }
    }
}
