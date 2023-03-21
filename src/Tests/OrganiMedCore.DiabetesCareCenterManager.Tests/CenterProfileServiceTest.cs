using Dapper;
using IntelliMed.Core.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.DiabetesCareCenterManager.Tests.MockServices;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.MockServices;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class CenterProfileServiceTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetCenterProfilesAsync_ShouldReturnAllCreated(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                // Not created
                var centerProfile1 = await service.NewCenterProfileAsync(111, true);
                var centerProfile2 = await service.NewCenterProfileAsync(112, true);
                var centerProfile3 = await service.NewCenterProfileAsync(113, true);
                var centerProfile4 = await service.NewCenterProfileAsync(114, true);

                // Create some
                await service.SaveCenterProfileAsync(centerProfile1, true);
                await service.SaveCenterProfileAsync(centerProfile4, true);

                // Temporarily save some
                await service.SaveCenterProfileAsync(centerProfile2, false);
                await service.SaveCenterProfileAsync(centerProfile3, false);

                var contentItems = await service.GetCenterProfilesAsync();

                Assert.True(contentItems.All(contentItem => contentItem.ContentType == ContentTypes.CenterProfile));
                Assert.True(contentItems.All(contentItem => contentItem.As<CenterProfilePart>().Created));
                Assert.True(contentItems.All(contentItem => contentItem.As<CenterProfilePart>().MemberRightId > 0));
                Assert.True(contentItems.All(contentItem => contentItem.Latest && contentItem.Published));
                Assert.Equal(2, contentItems.Count());
            });
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task NewCenterProfileAsync(bool cacheEnabled, bool shouldCreate)
        {
            var leaderId = 20001;

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new CenterProfileService(
                        null,
                        null,
                        new ClockMock(),
                        new ContentManagerMock(session),
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                    var centerProfile = await service.NewCenterProfileAsync(leaderId, shouldCreate);
                },
                async session =>
                {
                    var contentItem = await session
                        .Query<ContentItem, CenterProfilePartIndex>(index => index.MemberRightId == leaderId)
                        .FirstOrDefaultAsync();

                    if (shouldCreate)
                    {
                        Assert.NotNull(contentItem);
                    }
                    else
                    {
                        Assert.Null(contentItem);
                    }
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetCenterProfilesToLeaderAsync_ShouldReturnUsersCenterProfiles(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    null,
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var member1 = 500;
                var member2 = 300;

                await service.NewCenterProfileAsync(member1, true);
                await service.NewCenterProfileAsync(member2, true);
                await service.NewCenterProfileAsync(member2, true);

                var list = await service.GetCenterProfilesToLeaderAsync(member2);

                var ciIds = list.Select(x => x.ContentItemId).ToArray();

                var cp1 = await service.GetCenterProfileToLeaderAsync(member2, ciIds[0]);
                var cp2 = await service.GetCenterProfileToLeaderAsync(member2, ciIds[1]);
                var cpNull1 = await service.GetCenterProfileToLeaderAsync(member1, ciIds[0]);
                var cpNull2 = await service.GetCenterProfileToLeaderAsync(member1, ciIds[0]);

                Assert.NotEmpty(list);
                Assert.Equal(2, list.Count());
                Assert.True(list.All(ci => ci.ContentType == ContentTypes.CenterProfile));
                Assert.True(list.All(ci => ci.Latest && ci.Published));
                Assert.True(list.All(ci => ci.As<CenterProfilePart>().MemberRightId == member2));
                Assert.NotNull(cp1);
                Assert.NotNull(cp2);
                Assert.Null(cpNull1);
                Assert.Null(cpNull2);
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteCenterProfileAsync_ShouldBeDeleted(bool cacheEnabled)
        {
            var memberRightId = 300;
            var contentItemId = string.Empty;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    var cp1 = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    cp1.Alter<CenterProfilePart>(part => part.MemberRightId = memberRightId);
                    await contentManager.CreateAsync(cp1);

                    contentItemId = cp1.ContentItemId;
                },
                async session =>
                {
                    var service = new CenterProfileService(
                        null,
                        null,
                        null,
                        new ContentManagerMock(session),
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                    await service.DeleteCenterProfileAsync(contentItemId);
                },
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    Assert.Null(await contentManager.GetAsync(contentItemId));
                    Assert.NotNull(await contentManager.GetAsync(contentItemId, VersionOptions.Draft));
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteOwnCenterProfileAsync_ShouldThrownNotFoundException1(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    null,
                    null,
                    null,
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));
                await Assert.ThrowsAsync<NotFoundException>(() =>
                    service.DeleteOwnCenterProfileAsync("foo", 500));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteOwnCenterProfileAsync_ShouldThrownNotFoundException2(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    null,
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));
                var memberRightId = 500;
                var cp = await service.NewCenterProfileAsync(memberRightId, true);

                await Assert.ThrowsAsync<NotFoundException>(() =>
                    service.DeleteOwnCenterProfileAsync(cp.ContentItemId, ++memberRightId));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteOwnCenterProfileAsync_ShouldThrownCenterProfileAreadyCreatedException(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    new BetterUserServiceMock(1),
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var memberRightId = 500;
                var contentItem = await service.NewCenterProfileAsync(memberRightId, true);

                await service.SaveCenterProfileAsync(contentItem, true);

                await Assert.ThrowsAsync<CenterProfileAreadyCreatedException>(() =>
                    service.DeleteOwnCenterProfileAsync(contentItem.ContentItemId, memberRightId));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteOwnCenterProfileAsync_ShouldBeDeleted(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    null,
                    new BetterUserServiceMock(1),
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var memberRightId = 500;
                var contentItem = await service.NewCenterProfileAsync(memberRightId, true);

                await service.DeleteOwnCenterProfileAsync(contentItem.ContentItemId, memberRightId);

                contentItem = await service.GetCenterProfileToLeaderAsync(memberRightId, contentItem.ContentItemId);

                Assert.Null(contentItem);
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SaveCenterProfileAsync_WithViewModel_ShouldBeUpdated(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    null,
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var memberRightId = 501598;
                var contentItem = await service.NewCenterProfileAsync(memberRightId, true);

                // Updating basic data
                var centerName = "Test DCC";
                var basicData = new CenterProfileBasicDataViewModel() { CenterName = centerName };
                await service.SaveCenterProfileAsync(contentItem, basicData);

                // Updating additional data
                var workplaceCode = "abcd";
                var additionalData = new CenterProfileAdditionalDataViewModel()
                {
                    Neak = new List<CenterProfileNeakDataViewModel>()
                    {
                        new CenterProfileNeakDataViewModel()
                        {
                            Primary = true,
                            WorkplaceCode = workplaceCode
                        }
                    }
                };
                await service.SaveCenterProfileAsync(contentItem, additionalData);

                // Updating equipments
                var id1 = "a1";
                var equipments = new CenterProfileEquipmentsViewModel
                {
                    BackgroundConcilium = true,
                    BackgroundInpatient = true,
                    Laboratory = new List<CenterProfileEquipment<bool>>()
                    {
                        new CenterProfileEquipment<bool>() { Id = id1, Value = true }
                    },
                    Tools = new List<CenterProfileEquipment<int?>>()
                    {
                        new CenterProfileEquipment<int?>() { Id = id1, Value = 3 }
                    },
                };
                await service.SaveCenterProfileAsync(contentItem, equipments);

                contentItem = await service.GetCenterProfileAsync(contentItem.ContentItemId);

                var part = contentItem.As<CenterProfilePart>();

                Assert.NotNull(contentItem);
                Assert.Equal(centerName, part.CenterName);

                Assert.NotNull(part.Neak);
                Assert.True(part.Neak.First().Primary);
                Assert.Equal(workplaceCode, part.Neak.First().WorkplaceCode);

                Assert.True(part.BackgroundConcilium);
                Assert.True(part.BackgroundInpatient);
                Assert.Contains(part.Laboratory, x => x.Value && x.Id == id1);
                Assert.Contains(part.Tools, x => x.Value == 3 && x.Id == id1);
            });
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public async Task SaveCenterProfileAsync_SubmitNonCreated(bool cacheEnabled, bool submitted)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var leaderId = 20001;
                var centerProfile = await service.NewCenterProfileAsync(leaderId, true);

                await service.SaveCenterProfileAsync(centerProfile, submitted);

                centerProfile = (await service.GetCenterProfilesToLeaderAsync(leaderId)).FirstOrDefault();

                var part1 = centerProfile.As<CenterProfilePart>();
                var part2 = centerProfile.As<CenterProfileManagerExtensionsPart>();

                Assert.Equal(CenterProfileStatus.Unsubmitted, part2.RenewalCenterProfileStatus);
                if (submitted)
                {
                    Assert.True(part1.Created);
                }
            });
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task SaveCenterProfileAsync_SubmitCreated(bool cacheEnabled, bool submitted)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var leaderId = 20001;
                var centerProfile = await service.NewCenterProfileAsync(leaderId, true);

                await service.SaveCenterProfileAsync(centerProfile, true);

                centerProfile = (await service.GetCenterProfilesToLeaderAsync(leaderId)).FirstOrDefault();

                await service.SaveCenterProfileAsync(centerProfile, submitted);

                var part2 = centerProfile.As<CenterProfileManagerExtensionsPart>();

                Assert.Equal(
                    submitted ? CenterProfileStatus.UnderReviewAtTR : CenterProfileStatus.Unsubmitted,
                    part2.RenewalCenterProfileStatus);
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SetCenterProfileEquipmentSettingsAsync(bool cacheEnabled)
        {
            var service = new CenterProfileService(
                null,
                null,
                null,
                null,
                null,
                null,
                GetSignal(),
                await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

            await service.SetCenterProfileEquipmentSettingsAsync(new CenterProfileEquipmentsSettings()
            {
                ToolsList = new List<CenterProfileEquipmentSetting>()
                {
                    new CenterProfileEquipmentSetting() { Id ="a", Caption = "CA", Order = 1, Required = true, Type = EquipmentType.Boolean }
                },
                LaboratoryList = new List<CenterProfileEquipmentSetting>()
                {
                    new CenterProfileEquipmentSetting() { Id ="b", Caption = "CB", Order = 1, Required = false, Type = EquipmentType.Numeric }
                }
            });

            var settings = await service.GetCenterProfileEquipmentSettingsAsync();

            Assert.NotNull(settings);
            Assert.Contains(settings.ToolsList, x => x.Required && x.Type == EquipmentType.Boolean);
            Assert.Contains(settings.LaboratoryList, x => !x.Required && x.Type == EquipmentType.Numeric);
        }

        [Fact]
        public async Task ReviewAsync_ShouldThrowArgumentNullException()
        {
            var service = new CenterProfileService(null, null, null, null, null, null, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.ReviewAsync(null, true, ""));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.ReviewAsync(new CenterProfileReviewCheckResult(), true, ""));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ReviewAsync_AcceptWithourComment_ShouldThrownArgumentNullException(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var leaderId = 20001;
                // Not created
                var centerProfile = await service.NewCenterProfileAsync(leaderId, true);

                // Create:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                // Submit:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                await Assert.ThrowsAsync<ArgumentNullException>(() => service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.TerritorialRapporteur
                    },
                    false,
                    null));
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ReviewAsync_CenterProfileWithoutStatus(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var leaderId = 150238;
                // Not created
                var centerProfile = await service.NewCenterProfileAsync(leaderId, true);

                // Create:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.TerritorialRapporteur
                    },
                    true,
                    null);

                Assert.Equal(CenterProfileStatus.Unsubmitted, centerProfile.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus);
            });
        }

        [Theory]
        [InlineData(CenterPosts.TerritorialRapporteur, false, "NOK", true)]
        [InlineData(CenterPosts.TerritorialRapporteur, false, "NOK", false)]
        [InlineData(CenterPosts.TerritorialRapporteur, true, "OK", true)]
        [InlineData(CenterPosts.TerritorialRapporteur, true, "OK", false)]
        [InlineData(CenterPosts.MDTSecretary, false, "NOK", true)]
        [InlineData(CenterPosts.MDTSecretary, false, "NOK", false)]
        [InlineData(CenterPosts.MDTSecretary, true, null, true)]
        [InlineData(CenterPosts.MDTSecretary, true, null, false)]
        public async Task ReviewAsync_TerritorialRapporteur(string currentRole, bool accepted, string comment, bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var leaderId = 20001;
                // Not created
                var centerProfile = await service.NewCenterProfileAsync(leaderId, true);

                // Create:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                // Submit:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = currentRole
                    },
                    accepted,
                    comment);

                var status = centerProfile.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus;
                var reviewStates = centerProfile.As<CenterProfileReviewStatesPart>().States;
                var latestState = reviewStates.FirstOrDefault(x => x.Current);

                if (accepted)
                {
                    Assert.Equal(CenterProfileStatus.UnderReviewAtOMKB, status);
                    Assert.True(latestState.Accepted);
                    Assert.Null(latestState.Comment);
                }
                else
                {
                    Assert.Equal(CenterProfileStatus.Unsubmitted, status);
                    Assert.False(latestState.Accepted);
                    Assert.NotNull(latestState.Comment);
                }

                Assert.Equal(currentRole, latestState.Post);
            });
        }

        [Theory]
        [InlineData(false, "NOK", true)]
        [InlineData(false, "NOK", false)]
        [InlineData(true, null, true)]
        [InlineData(true, null, false)]
        public async Task ReviewAsync_OMKB(bool accepted, string comment, bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var leaderId = 20001;
                // Not created
                var centerProfile = await service.NewCenterProfileAsync(leaderId, true);

                // Create:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                // Submit:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                // TR Accept
                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.TerritorialRapporteur
                    },
                    true,
                    null);

                // OMKB:
                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.OMKB
                    },
                    accepted,
                    comment);

                var status = centerProfile.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus;
                var reviewStates = centerProfile.As<CenterProfileReviewStatesPart>().States;
                var latestState = reviewStates.FirstOrDefault(x => x.Current);

                if (accepted)
                {
                    Assert.Equal(CenterProfileStatus.UnderReviewAtMDT, status);
                    Assert.True(latestState.Accepted);
                    Assert.Null(latestState.Comment);
                }
                else
                {
                    Assert.Equal(CenterProfileStatus.Unsubmitted, status);
                    Assert.False(latestState.Accepted);
                    Assert.NotNull(latestState.Comment);
                }

                Assert.Equal(CenterPosts.OMKB, latestState.Post);
            });
        }

        [Theory]
        [InlineData(false, "NOK", true)]
        [InlineData(false, "NOK", false)]
        [InlineData(true, null, true)]
        [InlineData(true, null, false)]
        public async Task ReviewAsync_MDTManagement(bool accepted, string comment, bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var leaderId = 20001;
                // Not created
                var centerProfile = await service.NewCenterProfileAsync(leaderId, true);

                // Create:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                // Submit:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                // TR Accept
                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.TerritorialRapporteur
                    },
                    true,
                    null);

                // OMKB Accept
                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.OMKB
                    },
                    true,
                    null);

                // MDTManagement decision
                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.MDTManagement
                    },
                    accepted,
                    comment);

                var status = centerProfile.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus;
                var reviewStates = centerProfile.As<CenterProfileReviewStatesPart>().States;
                var latestState = reviewStates.FirstOrDefault(x => x.Current);

                if (accepted)
                {
                    Assert.Equal(CenterProfileStatus.MDTAccepted, centerProfile.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus);
                    Assert.NotEqual(AccreditationStatus.New, centerProfile.As<CenterProfilePart>().AccreditationStatus);
                    Assert.True(latestState.Accepted);
                    Assert.Null(latestState.Comment);
                }
                else
                {
                    Assert.Equal(
                        CenterProfileStatus.Unsubmitted,
                        centerProfile.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus);
                    Assert.False(latestState.Accepted);
                    Assert.NotNull(latestState.Comment);
                }

                Assert.Equal(CenterPosts.MDTManagement, latestState.Post);
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ReviewAsync_Accepted(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                var leaderId = 20001;
                // Not created
                var centerProfile = await service.NewCenterProfileAsync(leaderId, true);

                // Create:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                // Submit:
                await service.SaveCenterProfileAsync(centerProfile, true);
                centerProfile = await service.GetCenterProfileAsync(centerProfile.ContentItemId);

                // TR Accept
                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.TerritorialRapporteur
                    },
                    true,
                    null);

                // OMKB Accept
                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.OMKB
                    },
                    true,
                    null);

                // MDTManagement decision
                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.MDTManagement
                    },
                    true,
                    null);

                // MDTManagement decision
                await service.ReviewAsync(
                    new CenterProfileReviewCheckResult()
                    {
                        ContentItem = centerProfile,
                        CurrentRole = CenterPosts.MDTManagement
                    },
                    true,
                    null);

                var status = centerProfile.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus;
                var reviewStates = centerProfile.As<CenterProfileReviewStatesPart>().States;
                var latestState = reviewStates.FirstOrDefault(x => x.Current);

                Assert.Equal(CenterProfileStatus.MDTAccepted, centerProfile.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus);
                Assert.NotEqual(AccreditationStatus.New, centerProfile.As<CenterProfilePart>().AccreditationStatus);
                Assert.True(latestState.Accepted);
                Assert.Null(latestState.Comment);
                Assert.Equal(CenterPosts.MDTManagement, latestState.Post);
            });
        }

        [Fact]
        public async Task AcceptManyAsync_ShouldThrow_Exceptions()
        {
            var service = new CenterProfileService(null, null, null, null, null, null, null, null);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.AcceptManyAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => service.AcceptManyAsync(new CenterProfileDecisionStateViewModel[0]));
        }

        [Fact]
        public async Task AcceptManyAsync_ShouldThrow_ArgumentOutOfRangeException()
        {
            var service = new CenterProfileService(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                await SetupSiteAsync<CenterManagerSettings>(x =>
                {
                    x.CalculatedStatusOverridable = true;
                }));

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                service.AcceptManyAsync(new CenterProfileDecisionStateViewModel[]
                {
                    new CenterProfileDecisionStateViewModel()
                    {
                        AccreditationStatus = AccreditationStatus.New
                    }
                }));
        }

        [Fact]
        public Task AcceptManyAsync_ShouldThrow_ArgumentException()
        {
            var service = new CenterProfileService(null, null, null, null, null, null, null, null);

            return Assert.ThrowsAsync<ArgumentException>(() =>
                service.AcceptManyAsync(Enumerable.Empty<CenterProfileDecisionStateViewModel>()));
        }

        [Theory]
        // Enabling the status override
        [InlineData(true, AccreditationStatus.Accredited, AccreditationStatus.Accredited, true)]
        [InlineData(true, AccreditationStatus.Accredited, AccreditationStatus.Registered, true)]
        [InlineData(true, AccreditationStatus.Accredited, AccreditationStatus.TemporarilyAccredited, true)]
        [InlineData(false, AccreditationStatus.Accredited, AccreditationStatus.Accredited, true)]
        [InlineData(false, AccreditationStatus.Accredited, AccreditationStatus.Registered, true)]
        [InlineData(false, AccreditationStatus.Accredited, AccreditationStatus.TemporarilyAccredited, true)]

        // Enabling the status override but it is null, so will have the computed status
        [InlineData(true, AccreditationStatus.TemporarilyAccredited, null, true)]
        [InlineData(false, AccreditationStatus.TemporarilyAccredited, null, true)]

        // Disabling the status override
        [InlineData(true, AccreditationStatus.Registered, AccreditationStatus.Accredited, false)]
        [InlineData(true, AccreditationStatus.Registered, AccreditationStatus.TemporarilyAccredited, false)]
        [InlineData(true, AccreditationStatus.Registered, AccreditationStatus.Registered, false)]
        [InlineData(true, AccreditationStatus.Registered, null, false)]
        [InlineData(false, AccreditationStatus.Registered, AccreditationStatus.Accredited, false)]
        [InlineData(false, AccreditationStatus.Registered, AccreditationStatus.TemporarilyAccredited, false)]
        [InlineData(false, AccreditationStatus.Registered, AccreditationStatus.Registered, false)]
        [InlineData(false, AccreditationStatus.Registered, null, false)]
        public async Task AcceptManyAsync(bool cacheEnabled, AccreditationStatus calculatedStatus, AccreditationStatus? overriddenStatus, bool calculatedStatusOverridable)
        {
            var contentItemId = string.Empty;

            var clock = new ClockMock();
            var beforeUpdate = clock.UtcNow;
            var result = Enumerable.Empty<string>();

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);
                    var cp = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    cp.Alter<CenterProfileManagerExtensionsPart>(part =>
                    {
                        part.RenewalAccreditationStatus = calculatedStatus;
                        part.RenewalCenterProfileStatus = CenterProfileStatus.UnderReviewAtMDT;
                    });

                    await contentManager.CreateAsync(cp);

                    contentItemId = cp.ContentItemId;
                },
                async session =>
                {
                    var service = new CenterProfileService(
                        null,
                        null,
                        clock,
                        new ContentManagerMock(session),
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s =>
                        {
                            s.CalculatedStatusOverridable = calculatedStatusOverridable;
                            s.CenterProfileCacheEnabled = cacheEnabled;
                        }));

                    result = await service.AcceptManyAsync(new[]
                    {
                        new CenterProfileDecisionStateViewModel() { ContentItemId = contentItemId, AccreditationStatus = overriddenStatus }
                    });
                },
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    var cp = await contentManager.GetAsync(contentItemId);

                    var partCp = cp.As<CenterProfilePart>();
                    var partMg = cp.As<CenterProfileManagerExtensionsPart>();

                    if (calculatedStatusOverridable && overriddenStatus.HasValue)
                    {
                        Assert.Equal(overriddenStatus, partCp.AccreditationStatus);
                    }
                    else
                    {
                        Assert.Equal(calculatedStatus, partCp.AccreditationStatus);
                    }

                    Assert.Equal(CenterProfileStatus.MDTAccepted, partMg.RenewalCenterProfileStatus);
                    Assert.True(beforeUpdate < partCp.AccreditationStatusDateUtc && partCp.AccreditationStatusDateUtc < clock.UtcNow);
                });

            Assert.Contains(result, x => x == contentItemId);
        }

        [Theory]
        [InlineData(true, 99, new[] { 1032, 1033, 1034, 1035 })]
        [InlineData(false, 99, new[] { 1032, 1033, 1034, 1035 })]
        [InlineData(true, 55, new int[0])]
        [InlineData(false, 55, new int[0])]
        public async Task GetPermittedCenterProfilesAsync(bool cacheEnabled, int currentUserId, int[] zipCodesAssigned)
        {
            var permittedContentItemId = string.Empty;

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new CenterProfileService(
                        new AccreditationStatusCalculatorMock(),
                        null,
                        new ClockMock(),
                        new ContentManagerMock(session),
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                    // Creating  territories
                    var territory = new Territory() { Name = "X", TerritorialRapporteurId = currentUserId };
                    session.Save(territory);

                    // Creating settlements
                    foreach (var zipCode in zipCodesAssigned)
                    {
                        session.Save(new Settlement()
                        {
                            Name = "Bp",
                            ZipCode = zipCode,
                            TerritoryId = territory.Id
                        });

                        // Not created.
                        var centerProfile = await service.NewCenterProfileAsync(1, true);
                        permittedContentItemId = centerProfile.ContentItemId;

                        // Adding zipcode
                        centerProfile.Alter<CenterProfilePart>(part => part.CenterZipCode = zipCode);

                        // Create
                        await service.SaveCenterProfileAsync(centerProfile, true);

                        // Submit
                        await service.SaveCenterProfileAsync(centerProfile, true);
                    }
                },
                async session =>
                {
                    // If zipCodesAssigned is empty:
                    if (permittedContentItemId == string.Empty)
                    {
                        return;
                    }

                    var contentManager = new ContentManagerMock(session);
                    var newVersion = await contentManager.GetAsync(permittedContentItemId, VersionOptions.DraftRequired);
                    await contentManager.PublishAsync(newVersion);
                },
                async session =>
                {
                    var service = new CenterProfileService(
                        null,
                        new BetterUserServiceMock(currentUserId),
                        null,
                        null,
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                    var centerProfiles = await service.GetPermittedCenterProfilesAsync();

                    Assert.True(centerProfiles.All(contentItem => zipCodesAssigned.Contains(contentItem.As<CenterProfilePart>().CenterZipCode)));
                    Assert.True(centerProfiles.All(contentItem => contentItem.Published && contentItem.Latest));
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetPermittedCenterProfileAsync_ShouldThrow_UnauthorizedException(bool cacheEnabled)
        {
            var trId = 7778;
            var zipCodes = new int[] { 1009, 1015 };
            var permittedContentItemId = string.Empty;
            var notPermittedCotnentItemId = string.Empty;

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new CenterProfileService(
                        new AccreditationStatusCalculatorMock(),
                        null,
                        new ClockMock(),
                        new ContentManagerMock(session),
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                    // Creating  territories
                    var territory = new Territory() { Name = "TX", TerritorialRapporteurId = trId };
                    session.Save(territory);

                    // Creating settlements
                    foreach (var zipCode in zipCodes)
                    {
                        session.Save(new Settlement()
                        {
                            Name = "unknown city",
                            ZipCode = zipCode,
                            TerritoryId = territory.Id
                        });

                        // Not created.
                        var centerProfile = await service.NewCenterProfileAsync(1, true);
                        if (permittedContentItemId == string.Empty)
                        {
                            permittedContentItemId = centerProfile.ContentItemId;
                        }

                        // Adding zipcode
                        centerProfile.Alter<CenterProfilePart>(part => part.CenterZipCode = zipCode);

                        // Create
                        await service.SaveCenterProfileAsync(centerProfile, true);

                        // Submit
                        await service.SaveCenterProfileAsync(centerProfile, true);
                    }

                    // Creating another territory, settlement and profile
                    var territoryOther = new Territory() { TerritorialRapporteurId = 123, Name = "T2" };
                    session.Save(territoryOther);

                    session.Save(new Settlement() { Name = "__", ZipCode = 4444, TerritoryId = territoryOther.Id });

                    var centerProfile2 = await service.NewCenterProfileAsync(4, true);
                    centerProfile2.Alter<CenterProfilePart>(part => part.CenterZipCode = 4444);
                    // Create and submit
                    await service.SaveCenterProfileAsync(centerProfile2, true);
                    await service.SaveCenterProfileAsync(centerProfile2, true);

                    notPermittedCotnentItemId = centerProfile2.ContentItemId;
                },
                async session =>
                {
                    var service = new CenterProfileService(
                        null,
                        new BetterUserServiceMock(trId),
                        null,
                        null,
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                    await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetPermittedCenterProfileAsync(notPermittedCotnentItemId));
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetPermittedCenterProfileAsync(bool cacheEnabled)
        {
            var trId = 7778;
            var zipCodes = new int[] { 1009, 1015, 2489, 3034, 9271 };

            var permittedContentItemId = string.Empty;

            var siteService = await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled);

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new CenterProfileService(
                        new AccreditationStatusCalculatorMock(),
                        null,
                        new ClockMock(),
                        new ContentManagerMock(session),
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        siteService);

                    // Creating  territories
                    var territory = new Territory() { Name = "TX", TerritorialRapporteurId = trId };
                    session.Save(territory);

                    // Creating settlements
                    foreach (var zipCode in zipCodes)
                    {
                        session.Save(new Settlement()
                        {
                            Name = "unknown place",
                            ZipCode = zipCode,
                            TerritoryId = territory.Id
                        });

                        // Not created.
                        var centerProfile = await service.NewCenterProfileAsync(1, true);
                        if (permittedContentItemId == string.Empty)
                        {
                            permittedContentItemId = centerProfile.ContentItemId;
                        }

                        // Adding zipcode
                        centerProfile.Alter<CenterProfilePart>(part => part.CenterZipCode = zipCode);

                        // Create
                        await service.SaveCenterProfileAsync(centerProfile, true);

                        // Submit
                        await service.SaveCenterProfileAsync(centerProfile, true);
                    }
                },
                async session =>
                {
                    var service = new CenterProfileService(
                        null,
                        new BetterUserServiceMock(trId),
                        null,
                        null,
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        siteService);

                    var centerProfile = await service.GetPermittedCenterProfileAsync(permittedContentItemId);

                    Assert.NotNull(centerProfile);
                });
        }

        // Related to CenterProfileService.GetPermittedCenterProfileQueryAsync
        [Fact]
        public async Task GetPermittedCenterProfilesAsync_Query_ShouldBeOk()
        {
            var trId = 10;

            await RequestSessionsAsync(
                async session =>
                {
                    var territory = new Territory() { Name = "t1", TerritorialRapporteurId = trId };
                    session.Save(territory);

                    for (var i = 2500; i < 2600; i++)
                    {
                        session.Save(new Settlement()
                        {
                            ZipCode = i,
                            TerritoryId = territory.Id,
                            Name = "bla bla bla..."
                        });
                    }

                    var contentManager = new ContentManagerMock(session);
                    var random = new Random();
                    for (var i = 0; i < 10; i++)
                    {
                        var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                        contentItem.Alter<CenterProfilePart>(part =>
                        {
                            part.MemberRightId = random.Next(10000, 1000000);
                            part.AccreditationStatus = AccreditationStatus.New;
                            part.CenterZipCode = random.Next(2500, 2600);
                        });
                        contentItem.Alter<CenterProfileManagerExtensionsPart>(part => part.RenewalAccreditationStatus = AccreditationStatus.New);

                        await contentManager.CreateAsync(contentItem);
                    }
                },
               async session =>
               {
                   var sql =
                       $"SELECT DISTINCT ci.{nameof(ContentItemIndex.ContentItemId)} " +
                       $"FROM {nameof(TerritoryIndex)} t " +
                       $"JOIN {nameof(SettlementIndex)} s on s.{nameof(SettlementIndex.TerritoryId)} = t.DocumentId " +
                       $"JOIN {nameof(CenterProfilePartIndex)} c on c.{nameof(CenterProfilePartIndex.CenterZipCode)} = s.{nameof(SettlementIndex.ZipCode)} " +
                       $"JOIN {nameof(ContentItemIndex)} ci on ci.{nameof(ContentItemIndex.DocumentId)} = c.DocumentId " +
                       $"WHERE t.{nameof(TerritoryIndex.TerritorialRapporteurId)} = @territorialRapporteur";

                   var transaction = await session.DemandAsync();
                   var centerProfileContentItemIds = await transaction.Connection.QueryAsync<string>(sql, new { territorialRapporteur = trId }, transaction);

                   var centerProfiles = await session
                       .Query<ContentItem, CenterProfilePartIndex>(index =>
                           index.CenterZipCode >= 2500 &&
                           index.CenterZipCode < 2600)
                       .ListAsync();

                   Assert.Equal(10, centerProfileContentItemIds.Count());
                   Assert.Equal(10, centerProfiles.Count());
               });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetUnsubmittedCenterProfilesAsync1(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                // Not created
                var cp1 = await service.NewCenterProfileAsync(501, true);
                var cp2 = await service.NewCenterProfileAsync(502, true);
                var cp3 = await service.NewCenterProfileAsync(503, true);
                var cp4 = await service.NewCenterProfileAsync(504, true);
                var cp5 = await service.NewCenterProfileAsync(505, true);
                var cp6 = await service.NewCenterProfileAsync(506, true);

                // Create some
                await service.SaveCenterProfileAsync(cp1, true);
                await service.SaveCenterProfileAsync(cp3, true);
                await service.SaveCenterProfileAsync(cp5, true);
                await service.SaveCenterProfileAsync(cp6, true);

                // Submit some of the created ones.
                await service.SaveCenterProfileAsync(cp1, true);
                await service.SaveCenterProfileAsync(cp5, true);
                await service.SaveCenterProfileAsync(cp6, true);

                // Accepting, so the status will be UnderReviewAtOMKB
                await service.ReviewAsync(
                new CenterProfileReviewCheckResult()
                {
                    ContentItem = cp1,
                    CurrentRole = CenterPosts.TerritorialRapporteur
                },
                true,
                null);

                // Rejecting: the status will be Unsubmitted again.
                await service.ReviewAsync(
                new CenterProfileReviewCheckResult()
                {
                    ContentItem = cp6,
                    CurrentRole = CenterPosts.TerritorialRapporteur
                },
                false,
                "error");

                var unsubmittedOnes = await service.GetUnsubmittedCenterProfilesAsync(DateTime.UtcNow.AddHours(-1));

                Assert.Contains(unsubmittedOnes, cp => cp.ContentItemId == cp3.ContentItemId);
                Assert.Contains(unsubmittedOnes, cp => cp.ContentItemId == cp6.ContentItemId);

                Assert.DoesNotContain(unsubmittedOnes, cp => cp.ContentItemId == cp1.ContentItemId);
                Assert.DoesNotContain(unsubmittedOnes, cp => cp.ContentItemId == cp5.ContentItemId);
                Assert.DoesNotContain(unsubmittedOnes, cp => cp.ContentItemId == cp2.ContentItemId);
                Assert.DoesNotContain(unsubmittedOnes, cp => cp.ContentItemId == cp4.ContentItemId);
            });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChangeCenterProfileLeaderAsync(bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    null,
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                // Not created
                var cp1 = await service.NewCenterProfileAsync(501, true);

                var newLeaderId = 3000;
                await service.ChangeCenterProfileLeaderAsync(cp1, newLeaderId);

                var cp = await service.GetCenterProfileToLeaderAsync(newLeaderId, cp1.ContentItemId);

                Assert.Equal(newLeaderId, cp.As<CenterProfilePart>().MemberRightId);
            });
        }

        [Theory]
        [InlineData(false, false, false, 2, true)]
        [InlineData(false, false, false, 2, false)]
        [InlineData(true, false, false, 2, true)]
        [InlineData(true, false, false, 2, false)]
        [InlineData(true, true, false, 2, true)]
        [InlineData(true, true, false, 2, false)]
        [InlineData(true, true, true, 1, true)]
        [InlineData(true, true, true, 1, false)]
        public async Task GetUnsubmittedCenterProfilesAsync2(bool trAccept, bool omkbAccept, bool mdtAccept, int unsubmittedCount, bool cacheEnabled)
        {
            await RequestSessionsAsync(async session =>
            {
                var service = new CenterProfileService(
                    new AccreditationStatusCalculatorMock(),
                    null,
                    new ClockMock(),
                    new ContentManagerMock(session),
                    GetMemoryCache(),
                    session,
                    GetSignal(),
                    await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                // Not created
                var cp1 = await service.NewCenterProfileAsync(501, true);
                var cp2 = await service.NewCenterProfileAsync(502, true);
                var cp3 = await service.NewCenterProfileAsync(503, true);

                // Create
                await service.SaveCenterProfileAsync(cp1, true);
                await service.SaveCenterProfileAsync(cp2, true);

                // Submit
                await service.SaveCenterProfileAsync(cp1, true);
                await service.SaveCenterProfileAsync(cp2, false);

                if (trAccept)
                {
                    await service.ReviewAsync(new CenterProfileReviewCheckResult()
                    {
                        ContentItem = cp1,
                        CurrentRole = CenterPosts.TerritorialRapporteur
                    }, true, null);

                    if (omkbAccept)
                    {
                        await service.ReviewAsync(new CenterProfileReviewCheckResult()
                        {
                            ContentItem = cp1,
                            CurrentRole = CenterPosts.OMKB
                        },
                        true,
                        null);

                        await service.ReviewAsync(new CenterProfileReviewCheckResult()
                        {
                            ContentItem = cp1,
                            CurrentRole = CenterPosts.MDTManagement
                        },
                        mdtAccept,
                        "foo");
                    }
                    else
                    {
                        await service.ReviewAsync(new CenterProfileReviewCheckResult()
                        {
                            ContentItem = cp1,
                            CurrentRole = CenterPosts.OMKB
                        },
                        false,
                        "foo");
                    }
                }
                else
                {
                    await service.ReviewAsync(new CenterProfileReviewCheckResult()
                    {
                        ContentItem = cp1,
                        CurrentRole = CenterPosts.TerritorialRapporteur
                    }, false, "foo");
                }

                var unsubmitted = await service.GetUnsubmittedCenterProfilesAsync(DateTime.UtcNow.AddHours(-2));

                Assert.Equal(unsubmittedCount, unsubmitted.Count());
            });
        }

        [Theory]
        [InlineData("2004-02-03", "2005-03-15", true, false)]
        [InlineData("2004-02-03", "2005-03-15", true, true)]
        [InlineData("2005-01-01", "2005-02-01", true, false)]
        [InlineData("2005-01-01", "2005-02-01", true, true)]
        [InlineData("2005-01-01", "2005-01-02", true, false)]
        [InlineData("2005-01-01", "2005-01-02", true, true)]
        [InlineData("2018-03-05", "2018-03-01", false, false)]
        [InlineData("2018-03-05", "2018-03-01", false, true)]
        [InlineData("2018-03-05", "2018-03-05", false, false)]
        [InlineData("2018-03-05", "2018-03-05", false, true)]
        public async Task GetUnsubmittedCenterProfilesAsync_ShouldReturnPristine_AsExpected(
            string createDateAsString,
            string periodStartDateAsString,
            bool expectedResult,
            bool cacheEnabled)
        {
            var createDate = DateTime.Parse(createDateAsString);
            var periodStartDate = DateTime.Parse(periodStartDateAsString);

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    var cp = await contentManager.NewAsyncSpecifyCreateDate(ContentTypes.CenterProfile, createDate);

                    cp.Alter<CenterProfilePart>(part =>
                    {
                        part.Created = true;
                        part.MemberRightId = 1000;
                        part.AccreditationStatus = AccreditationStatus.TemporarilyAccredited;
                    });
                    cp.Alter<CenterProfileManagerExtensionsPart>(part =>
                    {
                        part.AccreditationStatusResult = null;
                    });

                    await contentManager.CreateAsync(cp);
                },
                async session =>
                {
                    var service = new CenterProfileService(
                        null,
                        null,
                        null,
                        null,
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                    var contentItems = (await service.GetUnsubmittedCenterProfilesAsync(periodStartDate)).ToArray();

                    Assert.Equal(expectedResult, contentItems.Any());
                });
        }

        [Theory]
        [InlineData(CenterProfileStatus.Unsubmitted, true, true)]
        [InlineData(CenterProfileStatus.Unsubmitted, true, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, false, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, false, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, false, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, false, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, false, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, false, false)]
        [InlineData(CenterProfileStatus.MDTAccepted, false, true)]
        [InlineData(CenterProfileStatus.MDTAccepted, false, false)]
        public async Task GetUnsubmittedCenterProfilesAsync_ShouldReturnUnsubmitted_AsExpected(
            CenterProfileStatus newStatus,
            bool expectedResult,
            bool cacheEnabled)
        {
            var periodStartDate = DateTime.UtcNow.AddDays(-1);
            var createDate = DateTime.UtcNow.AddYears(-1);

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    var cp = await contentManager.NewAsyncSpecifyCreateDate(ContentTypes.CenterProfile, createDate);
                    cp.Alter<CenterProfilePart>(part =>
                    {
                        part.Created = true;
                        part.MemberRightId = 1000;
                        part.AccreditationStatus = AccreditationStatus.Accredited;
                    });

                    await contentManager.CreateAsync(cp);

                    var cp1 = await contentManager.GetAsync(cp.ContentItemId, VersionOptions.DraftRequired);
                    cp1.Alter<CenterProfileManagerExtensionsPart>(part =>
                        part.RenewalCenterProfileStatus = newStatus);
                    await contentManager.PublishAsync(cp1);
                },
                async session =>
                {
                    var service = new CenterProfileService(
                        null,
                        null,
                        null,
                        null,
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                    var contentItems = await service.GetUnsubmittedCenterProfilesAsync(periodStartDate);

                    Assert.Equal(expectedResult, contentItems.Any());
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetUnsubmittedCenterProfilesAsync_MoreProfiles(bool cacheEnabled)
        {
            var random = new Random();
            var createDateBase = DateTime.UtcNow.AddYears(-1);
            var periodStartDate = DateTime.UtcNow.AddDays(-10);

            ContentItem cp1 = null;
            ContentItem cp2 = null;
            ContentItem cp3 = null;
            ContentItem cp4 = null;
            ContentItem cp5 = null;
            ContentItem cp6 = null;
            ContentItem cp7 = null;
            ContentItem cp8 = null;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    cp1 = await CreateCenterProfileAsync(contentManager, createDateBase.AddHours(random.Next(-24, 25)));
                    cp2 = await CreateCenterProfileAsync(contentManager, createDateBase.AddHours(random.Next(-24, 25)));
                    cp3 = await CreateCenterProfileAsync(contentManager, createDateBase.AddHours(random.Next(-24, 25)));
                    cp4 = await CreateCenterProfileAsync(contentManager, createDateBase.AddHours(random.Next(-24, 25)));
                    cp5 = await CreateCenterProfileAsync(contentManager, createDateBase.AddHours(random.Next(-24, 25)));
                    cp6 = await CreateCenterProfileAsync(contentManager, createDateBase.AddHours(random.Next(-24, 25)));
                    cp7 = await CreateCenterProfileAsync(contentManager, createDateBase.AddHours(random.Next(-24, 25)));
                    cp8 = await CreateCenterProfileAsync(contentManager, createDateBase.AddHours(random.Next(-24, 25)));
                },
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    await Task.WhenAll(
                        // Alter and submit:
                        AlterCenterProfile(contentManager, cp1, CenterProfileStatus.UnderReviewAtTR),
                        AlterCenterProfile(contentManager, cp3, CenterProfileStatus.UnderReviewAtOMKB),
                        AlterCenterProfile(contentManager, cp5, CenterProfileStatus.UnderReviewAtMDT),
                        AlterCenterProfile(contentManager, cp7, CenterProfileStatus.MDTAccepted),

                        // Alter but submit:
                        AlterCenterProfile(contentManager, cp2, CenterProfileStatus.Unsubmitted),
                        AlterCenterProfile(contentManager, cp8, CenterProfileStatus.Unsubmitted));

                    // No alteration, not submission in the renewal period for the cp4, cp6 ones.
                },
                async session =>
                {
                    var service = new CenterProfileService(
                        null,
                        null,
                        null,
                        null,
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled));

                    var contentItems = await service.GetUnsubmittedCenterProfilesAsync(periodStartDate);

                    Assert.DoesNotContain(contentItems, x => x.ContentItemId == cp1.ContentItemId);
                    Assert.DoesNotContain(contentItems, x => x.ContentItemId == cp3.ContentItemId);
                    Assert.DoesNotContain(contentItems, x => x.ContentItemId == cp5.ContentItemId);
                    Assert.DoesNotContain(contentItems, x => x.ContentItemId == cp7.ContentItemId);

                    Assert.Contains(contentItems, x => x.ContentItemId == cp2.ContentItemId);
                    Assert.Contains(contentItems, x => x.ContentItemId == cp4.ContentItemId);
                    Assert.Contains(contentItems, x => x.ContentItemId == cp6.ContentItemId);
                    Assert.Contains(contentItems, x => x.ContentItemId == cp8.ContentItemId);

                    Assert.Equal(
                        contentItems.Select(x => x.ContentItemId).Count(),
                        contentItems.Select(x => x.ContentItemId).Distinct().Count());
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetCenterProfileVersionsAsync(bool cacheEnabled)
        {
            var contentItemId = string.Empty;
            IEnumerable<ContentItem> versions = null;

            var siteService = await SetupSiteAsync<CenterManagerSettings>(s => s.CenterProfileCacheEnabled = cacheEnabled);

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new CenterProfileService(
                        new AccreditationStatusCalculatorMock(),
                        null,
                        new ClockMock(),
                        new ContentManagerMock(session),
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        siteService);

                    var leaderId = 123456;
                    // Not created
                    var centerProfile = await service.NewCenterProfileAsync(leaderId, true);

                    // Create:
                    await service.SaveCenterProfileAsync(centerProfile, true);

                    // Submit:
                    await service.SaveCenterProfileAsync(centerProfile, true);

                    contentItemId = centerProfile.ContentItemId;
                },
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    var newVersion = await contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);
                    await contentManager.PublishAsync(newVersion);

                    var newVersion2 = await contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);
                    await contentManager.PublishAsync(newVersion2);
                },
                async session =>
                {
                    var service = new CenterProfileService(null, null, null, null, GetMemoryCache(), session, GetSignal(), siteService);

                    versions = await service.GetCenterProfileVersionsAsync(contentItemId);

                    Assert.Equal(2, versions.Count());
                });
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task OnUserProfileUpdatedAsync_Colleague_ShouldRecalculate(bool cacheEnabled, bool testWithLeader)
        {
            const int leaderMemberRightId = 1234567;
            const int colleagueMemberRightId = 741205;
            const AccreditationStatus newAccreditationStatus = AccreditationStatus.TemporarilyAccredited;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);
                    var contentItem = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    contentItem.Alter<CenterProfilePart>(part =>
                    {
                        part.MemberRightId = leaderMemberRightId;
                        part.AccreditationStatus = AccreditationStatus.New;
                        part.Colleagues = new List<Colleague>()
                        {
                            new Colleague()
                            {
                                MemberRightId = colleagueMemberRightId
                            }
                        };
                    });
                    await contentManager.CreateAsync(contentItem);
                },
                async session =>
                {
                    var service = new CenterProfileService(
                        new AccreditationStatusCalculatorMock(newAccreditationStatus),
                        null,
                        null,
                        new ContentManagerMock(session),
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(x => x.CenterProfileCacheEnabled = cacheEnabled));

                    await service.OnUserProfilesUpdatedAsync(new[] { testWithLeader ? leaderMemberRightId : colleagueMemberRightId });
                },
                async session =>
                {
                    ContentItem contentItem;

                    if (testWithLeader)
                    {
                        contentItem = await session
                            .Query<ContentItem, CenterProfilePartIndex>(index => index.MemberRightId == leaderMemberRightId)
                            .LatestAndPublished()
                            .FirstOrDefaultAsync();
                    }
                    else
                    {
                        contentItem = await session
                            .Query<ContentItem, CenterProfileColleagueIndex>(index => index.MemberRightId == colleagueMemberRightId)
                            .LatestAndPublished()
                            .FirstOrDefaultAsync();
                    }

                    Assert.Equal(newAccreditationStatus, contentItem.As<CenterProfileManagerExtensionsPart>().RenewalAccreditationStatus);
                });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CalculateAccreditationStatus_ShouldRecalculate(bool cacheEnabled)
        {
            string id = null;
            var newStatus = AccreditationStatus.TemporarilyAccredited;
            var alterationText = "ABC";

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    var cp = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    cp.Alter<CenterProfilePart>(p => p.AccreditationStatus = AccreditationStatus.New);
                    cp.Alter<CenterProfileManagerExtensionsPart>(p => p.RenewalAccreditationStatus = AccreditationStatus.Registered);

                    await contentManager.CreateAsync(cp);

                    id = cp.ContentItemId;
                },
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    var service = new CenterProfileService(
                        new CalcMock(new AccreditationStatusResult() { AccreditationStatus = newStatus }),
                        null,
                        null,
                        contentManager,
                        GetMemoryCache(),
                        session,
                        GetSignal(),
                        await SetupSiteAsync<CenterManagerSettings>(x => x.CenterProfileCacheEnabled = cacheEnabled));

                    var cp = await contentManager.GetAsync(id);

                    await service.CalculateAccreditationStatusAsync(cp, x => x.AssignedTenantName = alterationText);
                },
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);
                    var cp = await contentManager.GetAsync(id);

                    var part = cp.As<CenterProfileManagerExtensionsPart>();

                    Assert.Equal(newStatus, part.RenewalAccreditationStatus);
                    Assert.Equal(newStatus, part.AccreditationStatusResult.AccreditationStatus);
                    Assert.Equal(alterationText, part.AssignedTenantName);
                });
        }


        private async Task<ContentItem> CreateCenterProfileAsync(ContentManagerMock contentManager, DateTime createDate)
        {
            var cp = await contentManager.NewAsyncSpecifyCreateDate(
                ContentTypes.CenterProfile,
                createDate);

            cp.Alter<CenterProfilePart>(part =>
            {
                part.Created = true;
                part.AccreditationStatus = AccreditationStatus.TemporarilyAccredited;
            });

            await contentManager.CreateAsync(cp);

            return cp;
        }

        private async Task AlterCenterProfile(
            ContentManagerMock contentManager,
            ContentItem contentItem,
            CenterProfileStatus newStatus)
        {
            var cp = await contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.DraftRequired);
            cp.Alter<CenterProfileManagerExtensionsPart>(part =>
                part.RenewalCenterProfileStatus = newStatus);
            await contentManager.PublishAsync(cp);
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
                        store.RegisterIndexes<CenterProfileColleagueIndexProvider>();
                        store.RegisterIndexes<CenterProfileManagerExtensionsPartIndexProvider>();
                        store.RegisterIndexes<TerritoryIndexProvider>();
                        store.RegisterIndexes<SettlementIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }

        private IMemoryCache GetMemoryCache() => new MemoryCache(new MemoryCacheOptions());

        private ISignal GetSignal() => new Signal();

        private async Task<ISiteService> SetupSiteAsync<T>(Action<T> action)
            where T : new()
        {
            var siteService = new SiteServiceMock();
            var site = await siteService.GetSiteSettingsAsync();
            site.Alter(typeof(T).Name, action);
            await siteService.UpdateSiteSettingsAsync(site);

            return siteService;
        }

        private class CalcMock : IAccreditationStatusCalculator
        {
            private readonly AccreditationStatusResult _result;


            public CalcMock(AccreditationStatusResult result)
            {
                _result = result;
            }


            public Task<AccreditationStatusResult> CalculateAccreditationStatusAsync(CenterProfilePart part)
                => Task.FromResult(_result);
        }
    }
}
