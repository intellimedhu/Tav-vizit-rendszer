using OrchardCore.ContentManagement.Records;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.DiabetesCareCenterManager.Settings;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using System;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class DccmCrossLoginHandlerTest
    {
        [Fact]
        public async Task GenerateNonceAsync_ShouldStore()
        {
            var memberRightId = 12345;

            var nonce = default(Guid);
            await RequestSessionsAsync(
                async session =>
                {
                    var handler = new DccmCrossLoginHandler(session);
                    nonce = await handler.GenerateNonceAsync(memberRightId);

                    Assert.NotEqual(Guid.Empty, nonce);
                },
                async session =>
                {
                    var dccmLogin = await session.Query<DccmLoginNonce>().FirstOrDefaultAsync();

                    Assert.Contains(dccmLogin.Storage.Keys, x => x == nonce);
                    Assert.Contains(dccmLogin.Storage.Values, x => x == memberRightId);
                });
        }

        [Fact]
        public async Task Validate_ShouldBeValidThenInvalid()
        {
            var memberRightId = 12345;
            int? result1 = null;
            int? result2 = null;

            await RequestSessionsAsync(
                session =>
                {
                    var dccmLogin = new DccmLoginNonce();
                    dccmLogin.Storage.Add(Guid.Empty, memberRightId);

                    session.Save(dccmLogin);

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var handler = new DccmCrossLoginHandler(session);
                    result1 = await handler.ValidateNonceAsync(Guid.Empty);
                },
                async session =>
                {
                    var handler = new DccmCrossLoginHandler(session);
                    result2 = await handler.ValidateNonceAsync(Guid.Empty);
                });

            Assert.NotNull(result1);
            Assert.Null(result2);
        }


        private async Task RequestSessionsAsync(params Func<ISession, Task>[] sessions)
        {
            using (var sessionHandler = new YesSqlSessionHandler())
            {
                await sessionHandler.InitializeAsync();
                await sessionHandler.RegisterSchemaAndIndexes(
                    schemaBuilder =>
                    {
                        ContentItemSchemaBuilder.Build(schemaBuilder);
                    },
                    store =>
                    {
                        store.RegisterIndexes<ContentItemIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }
    }
}
