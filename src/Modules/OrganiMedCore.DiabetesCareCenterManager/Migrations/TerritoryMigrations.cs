using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrchardCore.Data.Migration;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations
{
    public class TerritoryMigrations : DataMigration
    {
        public int Create()
        {
            TerritorySchemaBuilder.Build(SchemaBuilder);

            return 1;
        }
    }
}
