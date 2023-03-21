using System;
using System.Threading.Tasks;
using YesSql;
using YesSql.Provider.Sqlite;
using YesSql.Sql;

namespace OrganiMedCore.Testing.Core
{
    public class YesSqlSessionHandler : IDisposable
    {
        private TemporaryFolder _tempFolder;
        private IStore _store;


        public async Task InitializeAsync()
        {
            _tempFolder = new TemporaryFolder(true);
            _store = await StoreFactory.CreateAsync(config =>
            {
                config
                    .UseSqLite(@"Data Source=" + _tempFolder.Folder + "\\yessql.db;Cache=Shared")
                    .UseDefaultIdGenerator();
            });
        }

        public async Task RegisterSchemaAndIndexes(
            Action<SchemaBuilder> schemaBuilderRegistration = null,
            Action<IStore> indexRegistration = null)
        {
            using (var connection = _store.Configuration.ConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    var schemaBuilder = new SchemaBuilder(_store.Configuration, transaction);

                    schemaBuilderRegistration?.Invoke(schemaBuilder);

                    transaction.Commit();
                }
            }

            indexRegistration?.Invoke(_store);
        }

        public async Task RequestSessionsAsync(params Func<ISession, Task>[] useSessions)
        {
            foreach (var useSession in useSessions)
            {
                using (var session = _store.CreateSession())
                {
                    await useSession(session);
                }
            }
        }

        public void Dispose()
        {
            _store?.Dispose();
            _tempFolder?.Dispose();
        }
    }
}
