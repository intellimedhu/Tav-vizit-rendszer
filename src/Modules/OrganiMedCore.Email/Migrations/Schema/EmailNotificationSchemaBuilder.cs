using IntelliMed.Core.Extensions;
using OrganiMedCore.Email.Indexes;
using System;
using YesSql.Sql;

namespace OrganiMedCore.Email.Migrations.Schema
{
    public static class EmailNotificationSchemaBuilder
    {
        public static void Build(ISchemaBuilder schemaBuilder)
        {
            schemaBuilder.CreateMapIndexTable(nameof(EmailNotificationIndex), table => table
                .Column<string>(nameof(EmailNotificationIndex.NotificationId))
                .Column<string>(nameof(EmailNotificationIndex.To))
                .Column<DateTime>(nameof(EmailNotificationIndex.ScheduledSendDate))
                .Column<DateTime>(nameof(EmailNotificationIndex.SentOn))
                .Column<string>(nameof(EmailNotificationIndex.Cc))
                .Column<string>(nameof(EmailNotificationIndex.Bcc))
                .Column<string>(nameof(EmailNotificationIndex.TemplateId))
            );

            schemaBuilder.CreateColumnIndexes(nameof(EmailNotificationIndex),
                nameof(EmailNotificationIndex.NotificationId),
                nameof(EmailNotificationIndex.To),
                nameof(EmailNotificationIndex.ScheduledSendDate),
                nameof(EmailNotificationIndex.SentOn),
                nameof(EmailNotificationIndex.Cc),
                nameof(EmailNotificationIndex.Bcc),
                nameof(EmailNotificationIndex.TemplateId)
            );
        }
    }
}
