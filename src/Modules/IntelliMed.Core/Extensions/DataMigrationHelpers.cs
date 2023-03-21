using YesSql.Sql;

namespace IntelliMed.Core.Extensions
{
    public static class DataMigrationHelpers
    {
        /// <summary>
        /// Creates an index for all of the given columns.
        /// </summary>
        /// <param name="schemaBuilder"><see cref="SchemaBuilder"/></param>
        /// <param name="tableName">Name of the table will be used to prefix index names.</param>
        /// <param name="columnNames">Columns to index.</param>
        public static void CreateColumnIndexes(this ISchemaBuilder schemaBuilder, string tableName, params string[] columnNames)
        {
            if (columnNames.Length == 0)
            {
                return;
            }

            schemaBuilder.AlterTable(tableName, table =>
            {
                foreach (var column in columnNames)
                {
                    table.CreateIndex($"IDX_{tableName}_{column}", column);
                }
            });
        }
    }
}
