using OrchardCore.Data.Migration;
using OrganiMedCore.Email.Migrations.Schema;

namespace OrganiMedCore.Email.Migrations
{
    public class EmailNotificationMigrations : DataMigration
    {
        public int Create()
        {
            EmailNotificationSchemaBuilder.Build(SchemaBuilder);            

            return 1;
        }
    }
}
