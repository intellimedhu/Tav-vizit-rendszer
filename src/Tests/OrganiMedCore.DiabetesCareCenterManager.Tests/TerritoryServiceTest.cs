using IntelliMed.Core.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.MockServices;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class TerritoryServiceTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChangeTerritorialRapporteurAsync_ShouldThrow_NotFoundException(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new TerritoryService(
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                    UserManagerMock.Create<IUser>().Object);
                var territory = new Territory()
                {
                    TerritorialRapporteurId = 1,
                    Consultant = "c",
                    Name = "Area1"
                };
                session.Save(territory);

                await Assert.ThrowsAsync<NotFoundException>(() => service.ChangeTerritorialRapporteurAsync(territory.Id + 1, 2));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChangeTerritorialRapporteurAsync_ShouldThrow_TerritoryException(bool cacheEnabled)
        {
            var territory = new Territory() { TerritorialRapporteurId = 1 };

            await RequestSessionsAsync(
                session =>
                {
                    session.Save(territory);

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var service = new TerritoryService(
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                        null);

                    await Assert.ThrowsAsync<TerritoryException>(() =>
                        service.ChangeTerritorialRapporteurAsync(territory.Id, territory.TerritorialRapporteurId.Value));
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChangeTerritorialRapporteurAsync_ShouldReturnOk(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new TerritoryService(
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                    UserManagerMock.Create<IUser>().Object);
                var territory = new Territory()
                {
                    TerritorialRapporteurId = 1,
                    Consultant = "c",
                    Name = "Area1"
                };
                session.Save(territory);

                await service.ChangeTerritorialRapporteurAsync(territory.Id, 2);

                var t = await session.GetAsync<Territory>(territory.Id);

                Assert.Equal(2, t.TerritorialRapporteurId);
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetRapporteurToSettlementAsync_ShouldThrownSettlementNotFoundException(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var settlementService = new SettlementService(session);
                var zipCode = 1034;
                var settlementName = "Budapest";
                await settlementService.SaveSettlementAsync(new Settlement() { ZipCode = zipCode, Name = settlementName, Description = "A" });
                await settlementService.SaveSettlementAsync(new Settlement() { ZipCode = zipCode, Name = settlementName, Description = "B" });
                await settlementService.SaveSettlementAsync(new Settlement() { ZipCode = 1037, Name = "Budapest" });

                var service = new TerritoryService(
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                    UserManagerMock.Create<IUser>().Object);

                await Assert.ThrowsAsync<SettlementNotFoundException>(() =>
                    service.GetRapporteurToSettlementAsync(1032, settlementName));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetRapporteurToSettlementAsync_ShouldThrownSettlementHasNoTerritoryException(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var settlementService = new SettlementService(session);
                var zipCode = 1034;
                var settlementName = "Budapest";
                await settlementService.SaveSettlementAsync(new Settlement() { ZipCode = zipCode, Name = settlementName, Description = "A" });
                await settlementService.SaveSettlementAsync(new Settlement() { ZipCode = zipCode, Name = settlementName, Description = "B" });
                await settlementService.SaveSettlementAsync(new Settlement() { ZipCode = 1037, Name = "Budapest" });

                var service = new TerritoryService(
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                    UserManagerMock.Create<IUser>().Object);

                await Assert.ThrowsAsync<SettlementHasNoTerritoryException>(() =>
                    service.GetRapporteurToSettlementAsync(zipCode, settlementName));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetRapporteurToSettlementAsync_ShouldThrownTerritoryHasNoRapporteurException(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var territory = new Territory() { Name = "Óbuda", TerritorialRapporteurId = 1 };
                session.Save(territory);

                var settlementService = new SettlementService(session);
                var zipCode = 1034;
                var settlementName = "Budapest";
                await settlementService.SaveSettlementAsync(new Settlement()
                {
                    TerritoryId = territory.Id,
                    ZipCode = zipCode,
                    Name = settlementName,
                    Description = "A"
                });
                await settlementService.SaveSettlementAsync(new Settlement()
                {
                    TerritoryId = territory.Id,
                    ZipCode = zipCode,
                    Name = settlementName,
                    Description = "B"
                });
                await settlementService.SaveSettlementAsync(new Settlement()
                {
                    ZipCode = 1037,
                    Name = "Budapest"
                });

                var service = new TerritoryService(
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                    UserManagerMock.Create<IUser>().Object);

                await service.RemoveTerritoriesFromUserAsync(1);

                await Assert.ThrowsAsync<TerritoryHasNoRapporteurException>(() =>
                    service.GetRapporteurToSettlementAsync(zipCode, settlementName));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetRapporteurToSettlementAsync_ShouldReturnUser(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var tr = new User() { Email = "a@b.c" };

                session.Save(tr);

                var territory = new Territory() { Name = "Óbuda", TerritorialRapporteurId = tr.Id };
                session.Save(territory);

                var settlementService = new SettlementService(session);
                var zipCode = 1034;
                var settlementName = "Budapest";
                await settlementService.SaveSettlementAsync(new Settlement()
                {
                    TerritoryId = territory.Id,
                    ZipCode = zipCode,
                    Name = settlementName,
                    Description = "A"
                });
                await settlementService.SaveSettlementAsync(new Settlement()
                {
                    TerritoryId = territory.Id,
                    ZipCode = zipCode,
                    Name = settlementName,
                    Description = "B"
                });
                await settlementService.SaveSettlementAsync(new Settlement()
                {
                    ZipCode = 1037,
                    Name = "Budapest"
                });

                var service = new TerritoryService(
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                    UserManagerMock.Create<IUser>().Object);

                var user = await service.GetRapporteurToSettlementAsync(zipCode, settlementName);

                Assert.NotNull(user);
                Assert.Equal(tr.Id, user.Id);
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChangeConsultantAsync_ShouldThrow_NotFoundException(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new TerritoryService(
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                    null);
                await Assert.ThrowsAsync<NotFoundException>(() =>
                    service.ChangeConsultantAsync(100, "Kiss Géza"));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChangeConsultantAsync_ShouldThrow_TerritoryException(bool cacheEnabled)
        {
            var territory = new Territory()
            {
                Consultant = "test user",
                Name = "T1"
            };

            await RequestSessionsAsync(
                session =>
                {
                    session.Save(territory);

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var service = new TerritoryService(
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                        null);

                    await Assert.ThrowsAsync<TerritoryException>(() =>
                        service.ChangeConsultantAsync(territory.Id, territory.Consultant));
                });
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(false, null)]
        [InlineData(true, "")]
        [InlineData(false, "")]
        [InlineData(true, "new test user")]
        [InlineData(false, "new test user")]
        public async Task ChangeConsultantAsync_ShouldChange(bool cacheEnabled, string newConsultant)
        {
            var territory = new Territory()
            {
                Consultant = "test user",
                Name = "T1"
            };

            await RequestSessionsAsync(
                session =>
                {
                    session.Save(territory);

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var service = new TerritoryService(
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                        null);

                    await service.ChangeConsultantAsync(territory.Id, newConsultant);
                },
                async session =>
                {
                    var t = await session.GetAsync<Territory>(territory.Id);

                    Assert.Equal(newConsultant, t.Consultant);
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetZipCodesByTerritories(bool cacheEnabled)
        {
            var settlementZipCodes = new[] { 2000, 2001, 2002, 5000, 5001, 8000 };
            var cpZipCodes = new[] { 2001, 5000, 5001, 8000 };

            await RequestSessionsAsync(
                async session =>
                {
                    var t0 = new Territory() { Name = "T0" };
                    session.Save(t0);

                    var t1 = new Territory() { Name = "T1" };
                    session.Save(t1);

                    var t2 = new Territory() { Name = "T2" };
                    session.Save(t2);

                    var t3 = new Territory() { Name = "T3" };
                    session.Save(t3);

                    var random = new Random();
                    var settlements = new Settlement[settlementZipCodes.Length];

                    for (var i = 0; i < settlementZipCodes.Length; i++)
                    {
                        var r = random.Next(0, 3);
                        settlements[i] = new Settlement()
                        {
                            TerritoryId = (r == 0 ? t0 : r == 1 ? t1 : t2).Id,
                            ZipCode = settlementZipCodes[i],
                            Name = "n" + i
                        };

                        session.Save(settlements[i]);
                    }

                    var contentManager = new ContentManagerMock(session);
                    foreach (var zipCode in cpZipCodes)
                    {
                        var cp = await contentManager.NewAsync(ContentTypes.CenterProfile);
                        cp.Alter<CenterProfilePart>(part => part.CenterZipCode = zipCode);
                        await contentManager.CreateAsync(cp);
                    }
                },
                async session =>
                {
                    var service = new TerritoryService(
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                        null);

                    var result = await service.GetUsedZipCodesByTerritoriesAsync();

                    var missingZipCodes = settlementZipCodes.Except(cpZipCodes);
                    foreach (var zipCode in missingZipCodes)
                    {
                        Assert.DoesNotContain(result, x => x.Value.Contains(zipCode));
                    }

                    foreach (var zipCode in cpZipCodes)
                    {
                        Assert.Contains(result, x => x.Value.Contains(zipCode));
                    }
                });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TerritoryCache_ShouldCache(bool shouldEmptyCache)
        {
            const string originalName = "Original";
            var id = 0;

            var signal = GetSignal();
            var cache = GetMemoryCache();

            await RequestSessionsAsync(
                session =>
                {
                    var territory = new Territory() { Name = originalName };
                    session.Save(territory);

                    id = territory.Id;

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var service = new TerritoryService(
                        cache,
                        session,
                        signal,
                        await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = true),
                        null);

                    // Reads from database and writes to cache.
                    await service.GetTerritoryAsync(id);
                },
                async session =>
                {
                    // Updating without the service.
                    var territory = await session.GetAsync<Territory>(id);
                    territory.Name = "Updated";
                    session.Save(territory);

                    if (shouldEmptyCache)
                    {
                        var service = new TerritoryService(null, null, signal, null, null);
                        service.ClearTerritoryCache();
                    }
                },
                async session =>
                {
                    var service = new TerritoryService(
                        cache,
                        session,
                        signal,
                        await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = true),
                        null);

                    var territory = await service.GetTerritoryAsync(id);

                    Assert.Equal(!shouldEmptyCache, originalName == territory.Name);
                });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task RemoveTerritoriesFromUserAsync_ShouldRemoveAll(bool cacheEnabled)
        {
            var userId = 1007;
            var another = 509;

            await RequestSessionsAsync(
                session =>
                {
                    session.Save(new Territory() { TerritorialRapporteurId = userId });
                    session.Save(new Territory() { TerritorialRapporteurId = userId });
                    session.Save(new Territory() { TerritorialRapporteurId = userId });

                    session.Save(new Territory() { TerritorialRapporteurId = another });
                    session.Save(new Territory() { TerritorialRapporteurId = another });
                    session.Save(new Territory() { TerritorialRapporteurId = another });

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var service = new TerritoryService(
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.TerritoryCacheEnabled = cacheEnabled),
                        null);

                    await service.RemoveTerritoriesFromUserAsync(userId);
                },
                async session =>
                {
                    var territories = await session.Query<Territory>().ListAsync();

                    Assert.DoesNotContain(territories, t => t.TerritorialRapporteurId == userId);
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
                        ContentItemSchemaBuilder.Build(schemaBuilder);
                        CenterProfileSchemaBuilder.Build(schemaBuilder);
                        CenterProfileManagerExtensionSchemaBuilder.Build(schemaBuilder);
                        TerritorySchemaBuilder.Build(schemaBuilder);
                        SettlementSchemaBuilder.Build(schemaBuilder);
                    },
                    store =>
                    {
                        store.RegisterIndexes<ContentItemIndexProvider>();
                        store.RegisterIndexes<CenterProfilePartIndexProvider>();
                        store.RegisterIndexes<CenterProfileManagerExtensionsPartIndexProvider>();
                        store.RegisterIndexes<TerritoryIndexProvider>();
                        store.RegisterIndexes<SettlementIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }

        private async Task<ISiteService> SetupSiteAsync<T>(Action<T> action)
            where T : new()
        {
            var siteService = new SiteServiceMock();
            var site = await siteService.GetSiteSettingsAsync();
            site.Alter(typeof(T).Name, action);
            await siteService.UpdateSiteSettingsAsync(site);

            return siteService;
        }

        private IMemoryCache GetMemoryCache() => new MemoryCache(new MemoryCacheOptions());

        private ISignal GetSignal() => new Signal();
    }
}
