using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class CenterProfileReviewServiceTests
    {
        [Fact]
        public async Task GetAuthorizationResultAsync_ShouldThrow_ArgumentNullException()
        {
            var service = new CenterProfileReviewService(null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetAuthorizationResultAsync(null, null));

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetAuthorizationResultAsync(
                null,
                new ContentItem() { ContentType = "fake" }));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(CenterProfileStatus.Unsubmitted)]
        [InlineData(CenterProfileStatus.MDTAccepted)]
        public async Task GetAuthorizationResultAsync_NotAllowedStatus_ShouldReturnDefault(CenterProfileStatus? status)
        {
            var service = new CenterProfileReviewService(null, null);

            await RequestSessionsAsync(async session =>
            {
                var contentManager = new ContentManagerMock(session);

                var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalCenterProfileStatus = status);
                await contentManager.CreateAsync(contentItem);

                var result = await service.GetAuthorizationResultAsync(new User(), contentItem);

                Assert.Empty(result.CurrentRole);
                Assert.False(result.IsAuthorized);
            });
        }

        [Fact]
        public async Task GetAuthorizationResultAsync_UnauthorizedUser_ShouldReturnDefault()
        {
            var service = new CenterProfileReviewService(null, null);

            await RequestSessionsAsync(async session =>
            {
                var contentManager = new ContentManagerMock(session);

                var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtTR);
                await contentManager.CreateAsync(contentItem);

                var result = await service.GetAuthorizationResultAsync(new User(), contentItem);

                Assert.Empty(result.CurrentRole);
                Assert.False(result.IsAuthorized);
            });
        }

        [Theory]
        [InlineData(CenterPosts.MDTManagement, CenterProfileStatus.UnderReviewAtMDT)]
        [InlineData(CenterPosts.OMKB, CenterProfileStatus.UnderReviewAtOMKB)]
        public async Task GetAuthorizationResultAsync_MDTManagement_OMKB(string roleName, CenterProfileStatus status)
        {
            var service = new CenterProfileReviewService(null, null);

            await RequestSessionsAsync(async session =>
            {
                var contentManager = new ContentManagerMock(session);

                var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalCenterProfileStatus = status);
                await contentManager.CreateAsync(contentItem);

                var result = await service.GetAuthorizationResultAsync(
                    new User()
                    {
                        RoleNames = new List<string>() { roleName }
                    },
                    contentItem);

                Assert.Equal(roleName, result.CurrentRole);
                Assert.True(result.IsAuthorized);
            });
        }

        [Fact]
        public async Task GetAuthorizationResultAsync_TrOrSecretary_ShouldReturnDefault()
        {
            var service = new CenterProfileReviewService(null, null);

            await RequestSessionsAsync(async session =>
            {
                var contentManager = new ContentManagerMock(session);

                var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtTR);
                await contentManager.CreateAsync(contentItem);

                var result = await service.GetAuthorizationResultAsync(
                    new User()
                    {
                        RoleNames = new List<string>()
                    },
                    contentItem);

                Assert.Empty(result.CurrentRole);
                Assert.False(result.IsAuthorized);
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetAuthorizationResultAsync_TerritorialRapporteur_ShouldReturn_AsExpected(bool shouldFoundRapporteur)
        {
            var user1 = new User()
            {
                Id = 1,
                RoleNames = new List<string>() { CenterPosts.TerritorialRapporteur }
            };

            var user2 = new User()
            {
                Id = shouldFoundRapporteur ? 1 : 2,
                RoleNames = new List<string>() { CenterPosts.TerritorialRapporteur }
            };

            var service = new CenterProfileReviewService(
                new TerritoryServiceMock(user2),
                null);

            await RequestSessionsAsync(async session =>
            {
                var contentManager = new ContentManagerMock(session);

                var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                contentItem.Alter<CenterProfilePart>(part =>
                {
                    part.CenterZipCode = 1023;
                    part.CenterSettlementName = "Budapest";
                });
                contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtTR);
                await contentManager.CreateAsync(contentItem);

                var result = await service.GetAuthorizationResultAsync(user1, contentItem);

                if (shouldFoundRapporteur)
                {
                    Assert.Equal(CenterPosts.TerritorialRapporteur, result.CurrentRole);
                    Assert.True(result.IsAuthorized);
                }
                else
                {
                    Assert.Empty(result.CurrentRole);
                    Assert.False(result.IsAuthorized);
                }
            });
        }

        [Fact]
        public async Task GetAuthorizationResultAsync_MDTSecretary()
        {
            var user1 = new User()
            {
                Id = 1,
                RoleNames = new List<string>() { CenterPosts.MDTSecretary }
            };

            var service = new CenterProfileReviewService(new TerritoryServiceMock(true), null);

            await RequestSessionsAsync(async session =>
            {
                var contentManager = new ContentManagerMock(session);

                var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                contentItem.Alter<CenterProfilePart>(part =>
                {
                    part.CenterZipCode = 1023;
                    part.CenterSettlementName = "Budapest";
                });
                contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtTR);
                await contentManager.CreateAsync(contentItem);

                var result = await service.GetAuthorizationResultAsync(user1, contentItem);

                Assert.Equal(CenterPosts.MDTSecretary, result.CurrentRole);
                Assert.True(result.IsAuthorized);
            });
        }

        [Theory]
        [InlineData(null, CenterPosts.TerritorialRapporteur, false)]
        [InlineData(null, CenterPosts.OMKB, false)]
        [InlineData(null, CenterPosts.MDTSecretary, false)]
        [InlineData(null, CenterPosts.MDTManagement, false)]

        [InlineData(CenterProfileStatus.Unsubmitted, CenterPosts.TerritorialRapporteur, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, CenterPosts.OMKB, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, CenterPosts.MDTSecretary, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, CenterPosts.MDTManagement, false)]

        [InlineData(CenterProfileStatus.UnderReviewAtTR, CenterPosts.TerritorialRapporteur, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, CenterPosts.OMKB, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, CenterPosts.MDTSecretary, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, CenterPosts.MDTManagement, false)]

        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, CenterPosts.TerritorialRapporteur, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, CenterPosts.OMKB, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, CenterPosts.MDTSecretary, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, CenterPosts.MDTManagement, false)]

        [InlineData(CenterProfileStatus.UnderReviewAtMDT, CenterPosts.TerritorialRapporteur, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, CenterPosts.OMKB, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, CenterPosts.MDTSecretary, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, CenterPosts.MDTManagement, true)]

        [InlineData(CenterProfileStatus.MDTAccepted, CenterPosts.TerritorialRapporteur, false)]
        [InlineData(CenterProfileStatus.MDTAccepted, CenterPosts.OMKB, false)]
        [InlineData(CenterProfileStatus.MDTAccepted, CenterPosts.MDTSecretary, false)]
        [InlineData(CenterProfileStatus.MDTAccepted, CenterPosts.MDTManagement, false)]
        public async Task GetAuthorizationResultAsync_AllStatuses_AllRoles(CenterProfileStatus? status, string userRole, bool shouldBeAuthorized)
        {
            // Current user:
            var user = new User()
            {
                Id = 1,
                RoleNames = new List<string>() { userRole }
            };

            // If status is "UnderReviewAtTR" and userRole is "MDTSecretary" there should be another user to be the rapporteur.
            var user2 = new User()
            {
                Id = userRole == CenterPosts.MDTSecretary ? 2 : 1
            };

            var service = new CenterProfileReviewService(new TerritoryServiceMock(user2), null);

            await RequestSessionsAsync(async session =>
            {
                var contentManager = new ContentManagerMock(session);

                var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                contentItem.Alter<CenterProfilePart>(part =>
                {
                    part.CenterZipCode = 1023;
                    part.CenterSettlementName = "Budapest";
                });

                contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalCenterProfileStatus = status);
                await contentManager.CreateAsync(contentItem);

                var result = await service.GetAuthorizationResultAsync(user, contentItem);

                Assert.Equal(shouldBeAuthorized, result.IsAuthorized);
            });
        }

        [Fact]
        public async Task GetReviewerStatistics_UserIsNull_ShouldReturnEmpty()
        {
            var service = new CenterProfileReviewService(null, null);

            var result = await service.GetReviewerStatisticsAsync(null, new[] { new ContentItem() });

            Assert.Empty(result.ReviewableContentItemIds);
        }

        [Fact]
        public async Task GetReviewerStatistics_EmptyInput_ShouldReturnEmpty()
        {
            var service = new CenterProfileReviewService(null, null);

            var result = await service.GetReviewerStatisticsAsync(
                new User(),
                Enumerable.Empty<ContentItem>());

            Assert.Empty(result.ReviewableContentItemIds);
            Assert.Empty(result.ReviewStatisticsByRoles);
            Assert.Equal(0, result.TotalCount);
        }

        [Theory]
        [InlineData(CenterPosts.MDTManagement, CenterProfileStatus.UnderReviewAtMDT)]
        [InlineData(CenterPosts.OMKB, CenterProfileStatus.UnderReviewAtOMKB)]
        public async Task GetReviewerStatistics_RoleMDTManagement_RoleOMKB(string userRoles, CenterProfileStatus expectedStatus)
        {
            var contentItems = new List<ContentItem>();

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    contentItems.Add(
                        await CreateCenterProfileAsync(contentManager, CenterProfileStatus.MDTAccepted));

                    contentItems.Add(
                        await CreateCenterProfileAsync(contentManager, CenterProfileStatus.UnderReviewAtMDT));

                    contentItems.Add(
                        await CreateCenterProfileAsync(contentManager, CenterProfileStatus.UnderReviewAtOMKB));

                    contentItems.Add(
                        await CreateCenterProfileAsync(contentManager, CenterProfileStatus.UnderReviewAtTR));

                    contentItems.Add(
                        await CreateCenterProfileAsync(contentManager, CenterProfileStatus.Unsubmitted));

                    contentItems.Add(
                        await CreateCenterProfileAsync(contentManager));
                });

            var service = new CenterProfileReviewService(null, null);

            var expectedResult = contentItems
                .Where(contentItem => contentItem.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus == expectedStatus)
                .Select(contentItem => contentItem.ContentItemId);

            var result = await service.GetReviewerStatisticsAsync(
                new User()
                {
                    RoleNames = new List<string>() { userRoles }
                },
                contentItems);

            Assert.True(result.ReviewableContentItemIds.All(id => expectedResult.Contains(id)));
        }

        [Fact]
        public async Task GetReviewerStatistics_RoleMDTSecretary()
        {
            var contentItems = new List<ContentItem>();

            var id1 = string.Empty;
            var id2 = string.Empty;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    // Content item 1
                    var contentItem1 = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    contentItem1.Alter<CenterProfilePart>(part =>
                    {
                        part.CenterZipCode = 1023;
                        part.CenterSettlementName = "Bp";
                    });
                    contentItem1.Alter<CenterProfileManagerExtensionsPart>(part =>
                    {
                        part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtTR;
                    });
                    await contentManager.CreateAsync(contentItem1);

                    id1 = contentItem1.ContentItemId;
                    contentItems.Add(contentItem1);

                    // Content item 2
                    var contentItem2 = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    contentItem2.Alter<CenterProfilePart>(part =>
                    {
                        part.CenterZipCode = 3000;
                        part.CenterSettlementName = "Hatvan";
                    });
                    contentItem2.Alter<CenterProfileManagerExtensionsPart>(part =>
                    {
                        part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtTR;
                    });
                    await contentManager.CreateAsync(contentItem2);

                    id2 = contentItem2.ContentItemId;
                    contentItems.Add(contentItem2);

                    session.Save(new Settlement() { ZipCode = 1023, Name = "Bp", TerritoryId = 20 });
                    session.Save(new Settlement() { ZipCode = 3000, Name = "Hatvan", TerritoryId = 10 });
                },
                async session =>
                {
                    var service = new CenterProfileReviewService(
                        new TerritoryServiceMock(new[]
                        {
                            new Territory() { Id = 10, TerritorialRapporteurId = 11 },
                            new Territory() { Id = 20, TerritorialRapporteurId = null }
                        }),
                        session);

                    var result = await service.GetReviewerStatisticsAsync(
                        new User()
                        {
                            Id = 1,
                            RoleNames = new List<string>() { CenterPosts.MDTSecretary }
                        },
                        contentItems);

                    Assert.Contains(result.ReviewableContentItemIds, x => x == id1);
                    Assert.DoesNotContain(result.ReviewableContentItemIds, x => x == id2);
                });
        }

        [Fact]
        public async Task GetReviewerStatistics_RoleTerritorialRapporteur()
        {
            var contentItems = new List<ContentItem>();

            var id1 = string.Empty;
            var id2 = string.Empty;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    // Content item 1
                    var contentItem1 = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    contentItem1.Alter<CenterProfilePart>(part =>
                    {
                        part.CenterZipCode = 1023;
                        part.CenterSettlementName = "Bp";
                    });
                    contentItem1.Alter<CenterProfileManagerExtensionsPart>(part =>
                    {
                        part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtTR;
                    });
                    await contentManager.CreateAsync(contentItem1);

                    id1 = contentItem1.ContentItemId;
                    contentItems.Add(contentItem1);

                    // Content item 2
                    var contentItem2 = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    contentItem2.Alter<CenterProfilePart>(part =>
                    {
                        part.CenterZipCode = 3000;
                        part.CenterSettlementName = "Hatvan";
                    });
                    contentItem2.Alter<CenterProfileManagerExtensionsPart>(part =>
                    {
                        part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtTR;
                    });
                    await contentManager.CreateAsync(contentItem2);

                    id2 = contentItem2.ContentItemId;
                    contentItems.Add(contentItem2);

                    session.Save(new Settlement() { ZipCode = 1023, Name = "Bp", TerritoryId = 20 });
                    session.Save(new Settlement() { ZipCode = 3000, Name = "Hatvan", TerritoryId = 10 });
                },
                async session =>
                {
                    var service = new CenterProfileReviewService(
                        new TerritoryServiceMock(new[]
                        {
                            new Territory() { Id = 10, TerritorialRapporteurId = 11 },
                            new Territory() { Id = 20, TerritorialRapporteurId = 12 }
                        }),
                        session);

                    var result = await service.GetReviewerStatisticsAsync(
                        new User()
                        {
                            Id = 11,
                            RoleNames = new List<string>() { CenterPosts.TerritorialRapporteur }
                        },
                        contentItems);

                    Assert.Contains(result.ReviewableContentItemIds, x => x == id2);
                    Assert.DoesNotContain(result.ReviewableContentItemIds, x => x == id1);
                });
        }

        [Theory]
        [InlineData(CenterPosts.MDTManagement, 5, 7, 2, 3, 4, 1, 4, 17, 1, 22)]
        [InlineData(CenterPosts.OMKB, 5, 7, 2, 3, 4, 1, 3, 14, 5, 22)]
        [InlineData(CenterPosts.TerritorialRapporteur, 5, 7, 2, 3, 4, 1, 2, 12, 8, 22)]
        [InlineData(CenterPosts.MDTSecretary, 0, 7, 2, 3, 4, 1, 2, 7, 8, 17)]
        public async Task GetReviewerStatistics_WithOneRole_ShouldReturn_AsExpected(
            string userRole,

            int nullCount,
            int unsubmittedCount,
            int underReviewAtTrCount,
            int underReviewAtOmkbCount,
            int underReviewAtMdtCount,
            int mdtAcceptedCount,

            int expectedReviewableCount,
            int expectedNonReviewableCount,
            int expectedReviewedCount,
            int expectedTotalCount)
        {
            var userId = 20;
            var territory = new Territory()
            {
                TerritorialRapporteurId = 20
            };

            var s = new Settlement()
            {
                Name = "Bp",
                ZipCode = 1000
            };

            var expectedRole = userRole == CenterPosts.MDTSecretary
                ? CenterPosts.TerritorialRapporteur
                : userRole;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    for (var i = 0; i < nullCount; i++) await CreateCenterProfileAsync(contentManager, null, s.Name, s.ZipCode);
                    for (var i = 0; i < unsubmittedCount; i++) await CreateCenterProfileAsync(contentManager, CenterProfileStatus.Unsubmitted, s.Name, s.ZipCode);
                    for (var i = 0; i < underReviewAtTrCount; i++) await CreateCenterProfileAsync(contentManager, CenterProfileStatus.UnderReviewAtTR, s.Name, s.ZipCode);
                    for (var i = 0; i < underReviewAtOmkbCount; i++) await CreateCenterProfileAsync(contentManager, CenterProfileStatus.UnderReviewAtOMKB, s.Name, s.ZipCode);
                    for (var i = 0; i < underReviewAtMdtCount; i++) await CreateCenterProfileAsync(contentManager, CenterProfileStatus.UnderReviewAtMDT, s.Name, s.ZipCode);
                    for (var i = 0; i < mdtAcceptedCount; i++) await CreateCenterProfileAsync(contentManager, CenterProfileStatus.MDTAccepted, s.Name, s.ZipCode);

                    session.Save(territory);
                    if (userRole != CenterPosts.MDTSecretary)
                    {
                        s.TerritoryId = territory.Id;
                    }
                    session.Save(s);
                },
                async session =>
                {
                    var user = new User()
                    {
                        Id = userId,
                        RoleNames = new List<string>() { userRole }
                    };

                    var centerProfiles = await session
                        .Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentTypes.CenterProfile)
                        .ListAsync();

                    var service = new CenterProfileReviewService(
                        new TerritoryServiceMock(new[]
                        {
                            userRole != CenterPosts.MDTSecretary ? territory : new Territory()
                        }),
                        session);
                    var result = await service.GetReviewerStatisticsAsync(user, centerProfiles);

                    Assert.Single(result.ReviewStatisticsByRoles);
                    Assert.Equal(expectedRole, result.ReviewStatisticsByRoles.First().RoleName);
                    Assert.Equal(expectedTotalCount, result.TotalCount);
                    Assert.Equal(expectedReviewableCount, result.ReviewStatisticsByRoles.First().ReviewableCount);
                    Assert.Equal(expectedNonReviewableCount, result.ReviewStatisticsByRoles.First().NonReviewableCount);
                    Assert.Equal(expectedReviewedCount, result.ReviewStatisticsByRoles.First().ReviewedCount);
                });
        }


        private async Task<ContentItem> CreateCenterProfileAsync(
            ContentManagerMock contentManager,
            CenterProfileStatus? status = null,
            string settlementName = null,
            int? zipCode = null)
        {
            var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
            contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalCenterProfileStatus = status);
            if (!string.IsNullOrEmpty(settlementName) && zipCode.HasValue)
            {
                contentItem.Alter<CenterProfilePart>(part =>
                {
                    part.CenterSettlementName = settlementName;
                    part.CenterZipCode = zipCode.Value;
                });
            }
            await contentManager.CreateAsync(contentItem);

            return contentItem;
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


        private class TerritoryServiceMock : ITerritoryService
        {
            private readonly User _expectedResult;
            private readonly bool _shouldThrowTerritoryException;
            private readonly IEnumerable<Territory> _territories;


            public TerritoryServiceMock(User expectedResult)
            {
                _expectedResult = expectedResult;
            }

            public TerritoryServiceMock(bool shouldThrowTerritoryException)
            {
                _shouldThrowTerritoryException = shouldThrowTerritoryException;
            }

            public TerritoryServiceMock(IEnumerable<Territory> territories)
            {
                _territories = territories;
            }


            public Task<User> GetRapporteurToSettlementAsync(int zipCode, string settlementName)
            {
                if (_shouldThrowTerritoryException)
                {
                    throw new TerritoryException();
                }

                return Task.FromResult(_expectedResult);
            }

            public Task<IEnumerable<Territory>> GetTerritoriesAsync()
            {
                return Task.FromResult(_territories);
            }

            [ExcludeFromCodeCoverage]
            public Task<bool> CacheEnabledAsync()
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task ChangeConsultantAsync(int territoryId, string name)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task ChangeTerritorialRapporteurAsync(int territoryId, int newUserId)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public void ClearTerritoryCache()
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<IUser>> GetReviewersAsync(int zipCode, string settlementName)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<Territory> GetTerritoryAsync(int id)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IDictionary<Territory, IEnumerable<int>>> GetUsedZipCodesByTerritoriesAsync()
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task RemoveTerritoriesFromUserAsync(int userId)
                => throw new NotImplementedException();
        }
    }
}
