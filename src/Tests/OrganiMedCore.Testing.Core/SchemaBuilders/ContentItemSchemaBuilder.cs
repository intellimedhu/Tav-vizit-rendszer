using OrchardCore.ContentManagement.Records;
using System;
using YesSql.Sql;

namespace OrganiMedCore.Testing.Core.SchemaBuilders
{
    public static class ContentItemSchemaBuilder
    {
        public static void Build(SchemaBuilder schemaBuilder)
        {
            schemaBuilder.CreateMapIndexTable(nameof(ContentItemIndex), table => table
                    .Column<string>("ContentItemId", c => c.WithLength(26))
                    .Column<string>("ContentItemVersionId", c => c.WithLength(26))
                    .Column<bool>("Latest")
                    .Column<bool>("Published")
                    .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                    .Column<DateTime>("ModifiedUtc", column => column.Nullable())
                    .Column<DateTime>("PublishedUtc", column => column.Nullable())
                    .Column<DateTime>("CreatedUtc", column => column.Nullable())
                    .Column<string>("Owner", column => column.Nullable().WithLength(ContentItemIndex.MaxOwnerSize))
                    .Column<string>("Author", column => column.Nullable().WithLength(ContentItemIndex.MaxAuthorSize))
                    .Column<string>("DisplayText", column => column.Nullable().WithLength(ContentItemIndex.MaxDisplayTextSize))
                );

            schemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_ContentItemId", "ContentItemId", "Latest", "Published", "CreatedUtc")
            );

            schemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            schemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_DisplayText", "DisplayText")
            );
        }
    }
}
