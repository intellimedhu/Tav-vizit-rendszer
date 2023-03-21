using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.Testing.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class SettlementServiceTest
    {
        [Fact]
        public async Task SaveSettlementAsync_Add()
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new SettlementService(session);

                var zipCode = 3200;
                await service.SaveSettlementAsync(new Settlement()
                {
                    Description = "Petőfi Sándor út",
                    Name = "Gyöngyös",
                    ZipCode = zipCode,
                    TerritoryId = 97,
                });

                var settlements = await service.GetSettlementsAsync(97, 0);

                var index = await session.QueryIndex<SettlementIndex>(i => i.ZipCode == $"{zipCode}").FirstOrDefaultAsync();

                Assert.Single(settlements);
                Assert.Equal(zipCode, settlements.First().ZipCode);
                Assert.NotNull(index);
                Assert.Equal(zipCode, int.Parse(index.ZipCode));
            });
        }

        [Fact]
        public async Task SaveSettlementAsync_Update()
        {
            var territoryId = 18;
            var zipCode = 3200;

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new SettlementService(session);
                    await service.SaveSettlementAsync(new Settlement()
                    {
                        Description = "Petőfi Sándor út",
                        Name = "Gyöngyös",
                        ZipCode = zipCode,
                        TerritoryId = territoryId
                    });
                },
                async session =>
                {
                    var service = new SettlementService(session);

                    var settlements = await service.GetSettlementsAsync(territoryId, 0);

                    var settlement = settlements.First();

                    await service.SaveSettlementAsync(new Settlement()
                    {
                        Description = settlement.Description,
                        Name = settlement.Name,
                        Id = settlement.Id,
                        TerritoryId = settlement.TerritoryId,
                        ZipCode = ++zipCode
                    });
                },
                async session =>
                {
                    var service = new SettlementService(session);

                    var settlements = await service.GetSettlementsAsync(territoryId, 0);
                    var index = await session
                        .QueryIndex<SettlementIndex>(i => i.ZipCode == $"{zipCode}")
                        .FirstOrDefaultAsync();

                    Assert.Single(settlements);
                    Assert.Equal(1, await service.GetSettlementsCountAsync(territoryId, string.Empty));
                    Assert.Equal(1, await service.GetSettlementsCountAsync(territoryId, "Gy"));
                    Assert.Equal(0, await service.GetSettlementsCountAsync(territoryId, "Pécs"));
                    Assert.Equal(zipCode, settlements.First().ZipCode);
                    Assert.NotNull(index);
                    Assert.Equal(zipCode, int.Parse(index.ZipCode));
                });
        }

        [Fact]
        public async Task DeleteSettlementAsync()
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new SettlementService(session);
                await service.SaveSettlementAsync(new Settlement()
                {
                    ZipCode = 3200,
                    TerritoryId = 11
                });

                var settlements = await service.GetSettlementsAsync(11, 0);

                await service.DeleteSettlementAsync(settlements.First().Id);

                Assert.Empty(await service.GetSettlementsAsync(11, 0));
                Assert.Equal(0, await service.GetSettlementsCountAsync(11, null));
            });
        }

        [Theory]
        [InlineData(55, 0, 10)]
        [InlineData(103, 3, 10)]
        [InlineData(47, 3, 10)]
        [InlineData(55, 4, 10)]
        [InlineData(72, 5, 10)]
        [InlineData(44, 6, 0)]
        [InlineData(30, 2, 10)]
        [InlineData(30, 3, 0)]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 0)]
        public async Task GetSettlementsAsync(int count, int page, int expectedCount)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new SettlementService(session);

                var zipCode = 1001;
                for (var i = 0; i < count; i++)
                {
                    await service.SaveSettlementAsync(new Settlement()
                    {
                        Name = $"Bp - {i}",
                        ZipCode = zipCode++,
                        TerritoryId = 55
                    });
                }

                var settlements = await service.GetSettlementsAsync(55, page);

                Assert.Equal(expectedCount, settlements.Count());
                Assert.Equal(count, await service.GetSettlementsCountAsync(55, null));
            });
        }


        private async Task RequestSessionsAsync(params Func<ISession, Task>[] sessions)
        {
            using (var sessionHandler = new YesSqlSessionHandler())
            {
                await sessionHandler.InitializeAsync();
                await sessionHandler.RegisterSchemaAndIndexes(
                    schemaBuilder =>
                    {
                        TerritorySchemaBuilder.Build(schemaBuilder);
                        SettlementSchemaBuilder.Build(schemaBuilder);
                    },
                    store =>
                    {
                        store.RegisterIndexes<TerritoryIndexProvider>();
                        store.RegisterIndexes<SettlementIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }
    }
}
