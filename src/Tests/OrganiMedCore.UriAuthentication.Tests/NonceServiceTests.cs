using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.MockServices;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Services;
using OrganiMedCore.UriAuthentication.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.UriAuthentication.Tests
{
    public class NonceServiceTests
    {
        [Fact]
        public async Task Create_ShouldThrow()
        {
            var service = new NonceService(null, null, null, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAsync(new Nonce()));
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new Nonce() { RedirectUrl = "" }));
        }

        [Fact]
        public async Task Create_ShouldCreate()
        {
            var nonceType = NonceType.MemberRightId;
            var nonceRedirectUrl = "url";
            var nonceTypeId = 123;

            var nonce = new Nonce()
            {
                Type = nonceType,
                RedirectUrl = nonceRedirectUrl,
                TypeId = nonceTypeId
            };

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new NonceService(
                        new ClockMock(),
                        GetCache(),
                        session,
                        GetSignal(),
                        await SetupSiteServiceWithNonceSettings(settings => settings.NonceExpirationInDays = 1));

                    await service.CreateAsync(nonce);
                },
                async session =>
                {
                    var document = await session.Query<NonceDocument>().FirstOrDefaultAsync();
                    var result = document.Nonces.FirstOrDefault(x => x.Type == NonceType.MemberRightId);

                    Assert.NotNull(result);
                    Assert.Equal(nonceType, result.Type);
                    Assert.Equal(nonceRedirectUrl, result.RedirectUrl);
                    Assert.Equal(nonceTypeId, result.TypeId);
                    Assert.Equal(nonce.Value, result.Value);
                },
                async session =>
                {
                    var document = await session.Query<NonceDocument>().FirstOrDefaultAsync();
                    var result = document.Nonces.FirstOrDefault(x => x.TypeId == nonceTypeId);

                    Assert.NotNull(result);
                    Assert.Equal(nonceType, result.Type);
                    Assert.Equal(nonceRedirectUrl, result.RedirectUrl);
                    Assert.Equal(nonceTypeId, result.TypeId);
                    Assert.Equal(nonce.Value, result.Value);
                },
                async session =>
                {
                    var document = await session.Query<NonceDocument>().FirstOrDefaultAsync();
                    var result = document.Nonces.FirstOrDefault(x => x.RedirectUrl == nonceRedirectUrl);

                    Assert.NotNull(result);
                    Assert.Equal(nonceType, result.Type);
                    Assert.Equal(nonceRedirectUrl, result.RedirectUrl);
                    Assert.Equal(nonceTypeId, result.TypeId);
                    Assert.Equal(nonce.Value, result.Value);
                });

            Assert.NotEqual(default(Guid), nonce.Value);
        }

        [Fact]
        public async Task CreateMany_ShouldCreate()
        {
            var nonces = new[]
            {
                new Nonce() { RedirectUrl = "rd-1", Type = NonceType.MemberRightId, TypeId = 10 },
                new Nonce() { RedirectUrl = "rd-2", Type = NonceType.MemberRightId, TypeId = 20 },
                new Nonce() { RedirectUrl = "rd-3", Type = NonceType.MemberRightId, TypeId = 30 },
                new Nonce() { RedirectUrl = "rd-4", Type = NonceType.MemberRightId, TypeId = 40 }
            };

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new NonceService(
                        new ClockMock(),
                        GetCache(),
                        session,
                        GetSignal(),
                        await SetupSiteServiceWithNonceSettings(settings => settings.NonceExpirationInDays = 1));

                    await service.CreateManyAsync(nonces);
                },
                async session =>
                {
                    var document = await session.Query<NonceDocument>().FirstOrDefaultAsync();

                    Assert.Equal(nonces.Length, document.Nonces.Count);
                    Assert.True(document.Nonces.All(x => x.Value != default(Guid)));
                });
        }

        [Theory]
        [InlineData("10000000000000000000000000000001", "20000000000000000000000000000002", false)]
        [InlineData("10000000000000000000000000000001", "10000000000000000000000000000001", true)]
        public async Task GetByValue_ShouldReturnNull_AsExpected(Guid nonce1, Guid nonce2, bool shouldEqual)
        {
            var nonce = new Nonce()
            {
                Value = nonce1,
                RedirectUrl = "url",
                Type = NonceType.MemberRightId,
                TypeId = 11
            };

            await RequestSessionsAsync(
                session =>
                {
                    session.Save(new NonceDocument()
                    {
                        Nonces = new List<Nonce>() { nonce }
                    });

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var service = new NonceService(new ClockMock(), GetCache(), session, GetSignal(), null);
                    var result = await service.GetByValue(nonce2);

                    if (shouldEqual)
                    {
                        Assert.NotNull(result);
                        Assert.Equal(nonce.Value, result.Value);
                        Assert.Equal(nonce.RedirectUrl, result.RedirectUrl);
                        Assert.Equal(nonce.Type, result.Type);
                        Assert.Equal(nonce.TypeId, result.TypeId);
                    }
                    else
                    {
                        Assert.Null(result);
                    }
                });
        }

        [Theory]
        [InlineData("2007-05-05 10:00:22", "2007-05-05 10:00:21", false)]
        [InlineData("2007-05-05 10:00:22", "2007-05-05 10:00:22", true)]
        [InlineData("2007-05-05 10:00:22", "2007-05-05 10:00:23", true)]
        public async Task GetByValue_WithExpirationDate_ExpirationCheck_ShouldReturn_AsExpected(
            string utcNowAsString,
            string expirationDateAsString,
            bool shouldReturnNonce)
        {
            var nonceValue = Guid.NewGuid();

            await RequestSessionsAsync(
                session =>
                {
                    var document = new NonceDocument();
                    document.Nonces.Add(new Nonce()
                    {
                        ExpirationDate = DateTime.Parse(expirationDateAsString),
                        RedirectUrl = "rd",
                        Type = NonceType.MemberRightId,
                        TypeId = 1,
                        Value = nonceValue
                    });

                    session.Save(document);

                    return Task.CompletedTask;
                },
                async session =>
                {
                    var service = new NonceService(
                        new ClockMock2(DateTime.Parse(utcNowAsString)),
                        GetCache(),
                        session,
                        GetSignal(),
                        await SetupSiteServiceWithNonceSettings(s => s.NonceExpirationInDays = 1));

                    var nonce = await service.GetByValue(nonceValue);

                    Assert.Equal(shouldReturnNonce, nonce != null);
                });
        }

        [Theory]
        [InlineData("1997-11-02 11:00:15", 10, "1997-11-12 11:00:14", true)]
        [InlineData("1997-11-02 11:00:15", 10, "1997-11-12 11:00:15", true)]
        [InlineData("1997-11-02 11:00:15", 10, "1997-11-12 11:00:16", false)]
        public async Task GetByValue_WithoutExpirationDate_ExpirationCheck_ShouldReturn_AsExpected(
            string utcNowCreate,
            int expirationDays,
            string utcNowGet,
            bool shouldReturnNonce)
        {
            var nonceValue = default(Guid);

            var cache = GetCache();
            var signal = GetSignal();
            var siteService = await SetupSiteServiceWithNonceSettings(s => s.NonceExpirationInDays = expirationDays);

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new NonceService(new ClockMock2(DateTime.Parse(utcNowCreate)), cache, session, signal, siteService);

                    var nonce = new Nonce()
                    {
                        RedirectUrl = "r",
                        Type = NonceType.MemberRightId,
                        TypeId = 1
                    };
                    await service.CreateAsync(nonce);

                    nonceValue = nonce.Value;
                },
                async session =>
                {
                    var service = new NonceService(new ClockMock2(DateTime.Parse(utcNowGet)), cache, session, signal, siteService);

                    var nonce = await service.GetByValue(nonceValue);

                    Assert.Equal(shouldReturnNonce, nonce != null);
                });
        }

        [Theory]
        [InlineData("10000000000000000000000000000001", null, null)]
        [InlineData("10000000000000000000000000000001", "", null)]
        [InlineData("10000000000000000000000000000001", "https://www.very-nice-domain.com", "https://www.very-nice-domain.com/nc/10000000000000000000000000000001")]
        [InlineData("10000000000000000000000000000001", "https://www.very-nice-domain.com/", "https://www.very-nice-domain.com/nc/10000000000000000000000000000001")]
        public async Task GetUriAsync_ShouldReturnNull(string nonce, string siteUrl, string expectedResult)
        {
            var siteService = new SiteServiceMock();
            var settings = await siteService.GetSiteSettingsAsync();
            settings.BaseUrl = siteUrl;
            await siteService.UpdateSiteSettingsAsync(settings);
            var service = new NonceService(null, null, null, null, siteService);

            var uri = await service.GetUriAsync(Guid.Parse(nonce));

            Assert.Equal(expectedResult, uri);
        }

        [Fact]
        public async Task CleanupAsync_ShouldRemoveAllExpired()
        {
            var utcNowCreate = DateTime.Parse("1983-12-24 17:34");
            var expirationDays = 1;
            var utcNowGet = utcNowCreate.AddDays(expirationDays);

            var cache = GetCache();
            var signal = GetSignal();
            var siteService = await SetupSiteServiceWithNonceSettings(x => x.NonceExpirationInDays = expirationDays);

            await RequestSessionsAsync(
                session =>
                {
                    var service = new NonceService(null, cache, session, signal, siteService);
                    return service.CreateManyAsync(new[]
                    {
                        // Should be removed
                        // utcNowCreate + 5 hr < utcNowGet
                        new Nonce() { RedirectUrl = "x", Type = NonceType.MemberRightId, TypeId = 1, ExpirationDate = utcNowCreate.AddHours(5) },
                        new Nonce() { RedirectUrl = "x", Type = NonceType.MemberRightId, TypeId = 2, ExpirationDate = utcNowCreate.AddHours(5) },
                        new Nonce() { RedirectUrl = "x", Type = NonceType.MemberRightId, TypeId = 3, ExpirationDate = utcNowCreate.AddHours(5) },
                        new Nonce() { RedirectUrl = "x", Type = NonceType.MemberRightId, TypeId = 4, ExpirationDate = utcNowCreate.AddHours(5) },

                        // Should keep
                        // utcNowCreate + 2 days > utcNowGet
                        new Nonce()
                        {
                            RedirectUrl = "y",
                            Type = NonceType.MemberRightId,
                            TypeId = 5,
                            ExpirationDate = utcNowCreate.AddDays(expirationDays + 1)
                        },
                    });
                },
                session =>
                {
                    var service = new NonceService(new ClockMock2(utcNowGet), cache, session, signal, siteService);

                    return service.CleanupAsync();
                },
                async session =>
                {
                    var document = await session.Query<NonceDocument>().FirstOrDefaultAsync();

                    Assert.DoesNotContain(document.Nonces, x => x.RedirectUrl == "x");
                });
        }


        private async Task RequestSessionsAsync(params Func<ISession, Task>[] sessions)
        {
            using (var sessionHandler = new YesSqlSessionHandler())
            {
                await sessionHandler.InitializeAsync();
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }

        private IMemoryCache GetCache() => new MemoryCache(new MemoryCacheOptions());

        private ISignal GetSignal() => new Signal();

        private async Task<SiteServiceMock> SetupSiteServiceWithNonceSettings(Action<NonceSettings> action)
        {
            var siteService = new SiteServiceMock();
            var site = await siteService.GetSiteSettingsAsync();
            site.Alter(nameof(NonceSettings), action);
            await siteService.UpdateSiteSettingsAsync(site);

            return siteService;
        }


        private class ClockMock2 : IClock
        {
            public DateTime UtcNow { get; }


            public ClockMock2(DateTime expectedUtcNow)
            {
                UtcNow = expectedUtcNow;
            }


            [ExcludeFromCodeCoverage]
            public DateTimeOffset ConvertToTimeZone(DateTimeOffset dateTimeOffset, ITimeZone timeZone)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public ITimeZone GetSystemTimeZone()
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public ITimeZone GetTimeZone(string timeZoneId)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public ITimeZone[] GetTimeZones()
                => throw new NotImplementedException();
        }
    }
}
