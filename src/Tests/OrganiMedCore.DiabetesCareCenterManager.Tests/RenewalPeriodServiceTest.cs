using IntelliMed.DokiNetIntegration.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
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
using OrganiMedCore.Email.Indexes;
using OrganiMedCore.Email.Migrations.Schema;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Testing.Core;
using OrganiMedCore.Testing.Core.MockServices;
using OrganiMedCore.Testing.Core.SchemaBuilders;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class RenewalPeriodServiceTest
    {
        #region Notifications

        [Fact]
        public async Task QueueNotificationsAboutUnsubmittedCenterProfile_ShouldLogNullSettings()
        {
            var logger = new LoggerMock<RenewalPeriodService>();

            var service = new RenewalPeriodService(
                null,
                null,
                null,
                null,
                null,
                logger,
                null,
                new RenewalPeriodSettingsService_RenewalPeriodService(null));

            await service.QueueNotificationsAboutUnsubmittedCenterProfile(DateTime.UtcNow);

            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("RenewalSettings is null."));
        }

        [Fact]
        public async Task QueueNotificationsAboutUnsubmittedCenterProfile_ShouldLogNoActiveRenewalPeriod()
        {
            var logger = new LoggerMock<RenewalPeriodService>();

            var renewalSettings = new CenterRenewalSettings();
            renewalSettings.RenewalPeriods.Add(new RenewalPeriod()
            {
                RenewalStartDate = DateTime.Parse("2019-01-01"),
                ReviewStartDate = DateTime.Parse("2019-02-01")
            });
            var now = DateTime.Parse("2019-02-10");

            var service = new RenewalPeriodService(
                null,
                null,
                null,
                null,
                null,
                logger,
                null,
                new RenewalPeriodSettingsService_RenewalPeriodService(renewalSettings));

            await service.QueueNotificationsAboutUnsubmittedCenterProfile(now);

            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("No active renewal period."));
        }

        [Fact]
        public async Task QueueNotificationsAboutUnsubmittedCenterProfile_ShouldLogProcessed()
        {
            var logger = new LoggerMock<RenewalPeriodService>();

            var t1 = DateTime.Parse("2019-01-10 10:00");
            var t2 = DateTime.Parse("2019-01-15 20:00");

            var renewalSettings = new CenterRenewalSettings();
            renewalSettings.RenewalPeriods.Add(new RenewalPeriod()
            {
                RenewalStartDate = DateTime.Parse("2019-01-01"),
                ReviewStartDate = DateTime.Parse("2019-02-01"),
                EmailTimings = new List<DateTime>() { t1, t2 },
                ProcessedTimings = new List<DateTime>() { t1, t2 }
            });

            var now = DateTime.Parse("2019-01-18");

            var service = new RenewalPeriodService(
                null,
                null,
                null,
                null,
                null,
                logger,
                null,
                new RenewalPeriodSettingsService_RenewalPeriodService(renewalSettings));

            await service.QueueNotificationsAboutUnsubmittedCenterProfile(now);

            Assert.Contains(logger.Logs, log => log.State.ToString().StartsWith("All timing dates have been already processed"));
        }

        [Fact]
        public async Task QueueNotificationsAboutUnsubmittedCenterProfile_ShouldLogNoActiveTiming()
        {
            var logger = new LoggerMock<RenewalPeriodService>();

            var t1 = DateTime.Parse("2019-01-10 10:00");
            var t2 = DateTime.Parse("2019-01-20 22:00");

            var renewalSettings = new CenterRenewalSettings();
            renewalSettings.RenewalPeriods.Add(new RenewalPeriod()
            {
                RenewalStartDate = DateTime.Parse("2019-01-01"),
                ReviewStartDate = DateTime.Parse("2019-02-01"),
                EmailTimings = new List<DateTime>() { t1, t2 },
                ProcessedTimings = new List<DateTime>() { t1 }
            });

            var now = DateTime.Parse("2019-01-18 17:00");

            var service = new RenewalPeriodService(
                null,
                null,
                null,
                null,
                null,
                logger,
                null,
                new RenewalPeriodSettingsService_RenewalPeriodService(renewalSettings));

            await service.QueueNotificationsAboutUnsubmittedCenterProfile(now);

            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("No active timing."));
        }

        [Fact]
        public async Task QueueNotificationsAboutUnsubmittedCenterProfile_ShouldLogNoUnsubmittedCenterProfiles()
        {
            var logger = new LoggerMock<RenewalPeriodService>();

            var t1 = DateTime.Parse("2019-01-10 10:00");
            var t2 = DateTime.Parse("2019-01-15 20:00");

            var renewalSettings = new CenterRenewalSettings();
            renewalSettings.RenewalPeriods.Add(new RenewalPeriod()
            {
                RenewalStartDate = DateTime.Parse("2019-01-01"),
                ReviewStartDate = DateTime.Parse("2019-02-01"),
                EmailTimings = new List<DateTime>() { t1, t2 },
                ProcessedTimings = new List<DateTime>() { t1 }
            });

            var now = DateTime.Parse("2019-01-18");

            var service = new RenewalPeriodService(
                new CenterProfileService_RenewalPeriodService(true, null),
                null,
                null,
                null,
                new EmailTemplateProviderMock(new EmailTemplate()),
                logger,
                null,
                new RenewalPeriodSettingsService_RenewalPeriodService(renewalSettings));

            await service.QueueNotificationsAboutUnsubmittedCenterProfile(now);

            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("No unsubmitted cp."));
        }

        [Fact]
        public async Task QueueNotificationsAboutUnsubmittedCenterProfile_ShouldLogNoTemplate()
        {
            var logger = new LoggerMock<RenewalPeriodService>();

            var t1 = DateTime.Parse("2019-01-10 10:00");
            var t2 = DateTime.Parse("2019-01-15 20:00");

            var renewalSettings = new CenterRenewalSettings();
            renewalSettings.RenewalPeriods.Add(new RenewalPeriod()
            {
                RenewalStartDate = DateTime.Parse("2019-01-01"),
                ReviewStartDate = DateTime.Parse("2019-02-01"),
                EmailTimings = new List<DateTime>() { t1, t2 },
                ProcessedTimings = new List<DateTime>() { t1 }
            });

            var now = DateTime.Parse("2019-01-18");

            var renewalPeriodSettingService = new RenewalPeriodSettingsService_RenewalPeriodService(renewalSettings);

            var service = new RenewalPeriodService(
                new CenterProfileService_RenewalPeriodService(true, null),
                null,
                null,
                null,
                new EmailTemplateProviderMock(null),
                logger,
                null,
                renewalPeriodSettingService);

            await service.QueueNotificationsAboutUnsubmittedCenterProfile(now);
            var updatedSettings = await renewalPeriodSettingService.GetCenterRenewalSettingsAsync();

            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("No template has been set. Skipping current timing."));
            // No template but t2 should be in the 'processed' list.
            Assert.Contains(updatedSettings.LatestFullPeriod.ProcessedTimings, x => x.Equals(t2));
        }

        [Fact]
        public async Task QueueNotificationsAboutUnsubmittedCenterProfile_ShouldQueueNotifications()
        {
            var now = DateTime.Parse("2019-01-15 17:00");

            var logger = new LoggerMock<RenewalPeriodService>();
            var renewalSettings = new CenterRenewalSettings();

            await RequestSessionsAsync(
                async session =>
                {
                    var t1 = DateTime.Parse("2019-01-15 16:30");

                    renewalSettings.RenewalPeriods.Add(new RenewalPeriod()
                    {
                        Id = Guid.NewGuid(),
                        RenewalStartDate = DateTime.Parse("2016-12-15"),
                        ReviewStartDate = DateTime.Parse("2017-01-16")
                    });
                    renewalSettings.RenewalPeriods.Add(new RenewalPeriod()
                    {
                        Id = Guid.NewGuid(),
                        RenewalStartDate = DateTime.Parse("2019-01-01"),
                        ReviewStartDate = DateTime.Parse("2019-02-01"),
                        EmailTimings = new List<DateTime>() { t1 }
                    });
                    renewalSettings.RenewalPeriods.Add(new RenewalPeriod()
                    {
                        Id = Guid.NewGuid(),
                        RenewalStartDate = DateTime.Parse("2019-12-10"),
                        ReviewStartDate = DateTime.Parse("2020-02-04")
                    });

                    var service = new RenewalPeriodService(
                        new CenterProfileService_RenewalPeriodService(false, session),
                        null,
                        new DokiNetServiceMock(),
                        new EmailNotificationDataService_RenewalPeriodService(session),
                        new EmailTemplateProviderMock(new EmailTemplate()),
                        logger,
                        new NonceServiceMock(),
                        new RenewalPeriodSettingsService_RenewalPeriodService(renewalSettings));

                    await service.QueueNotificationsAboutUnsubmittedCenterProfile(now);
                });

            Assert.Contains(logger.Logs, log => log.State.ToString().StartsWith("Queue email"));
            Assert.True(renewalSettings[now].ProcessedTimings.Any());
        }

        #endregion

        #region Resetting statuses

        [Fact]
        public async Task ResetCenterProfileStatusesAsync_ShouldLog_RenewalSettingsIsNull()
        {
            var logger = new LoggerMock<RenewalPeriodService>();

            var service = new RenewalPeriodService(
                null,
                null,
                null,
                null,
                null,
                logger,
                null,
                new RenewalPeriodSettingsServiceMock(null));

            await service.ResetCenterProfileStatusesAsync(DateTime.UtcNow);

            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("RenewalSettings is null."));
        }

        [Theory]
        [InlineData("2010-01-07 22:00", "2010-01-06 23:59")]
        [InlineData("2010-01-07 16:16", "2010-01-08 00:00")]
        public async Task ResetCenterProfileStatusesAsync_ShouldLog_NoRenewalPeriod(string renewalStartDateUtcAsString, string utcNowAsString)
        {
            var logger = new LoggerMock<RenewalPeriodService>();
            var utcNow = DateTime.SpecifyKind(DateTime.Parse(utcNowAsString), DateTimeKind.Utc);

            await RequestSessionsAsync(
                async session =>
                {
                    var service = new RenewalPeriodService(
                        null,
                        null,
                        null,
                        null,
                        null,
                        logger,
                        null,
                        new RenewalPeriodSettingsServiceMock(new CenterRenewalSettings()
                        {
                            RenewalPeriods = new[]
                            {
                                new RenewalPeriod() { RenewalStartDate = DateTime.SpecifyKind(DateTime.Parse(renewalStartDateUtcAsString), DateTimeKind.Utc) }
                            }
                        }));

                    await service.ResetCenterProfileStatusesAsync(utcNow);
                });

            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("There is no renewal period that begins today"));
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(CenterProfileStatus.MDTAccepted, true)]
        [InlineData(CenterProfileStatus.UnderReviewAtMDT, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtOMKB, false)]
        [InlineData(CenterProfileStatus.UnderReviewAtTR, false)]
        [InlineData(CenterProfileStatus.Unsubmitted, false)]
        public async Task ResetCenterProfileStatusesAsync(CenterProfileStatus? centerProfileStatus, bool shouldReset)
        {
            var originalAccreditationStatus = AccreditationStatus.Accredited;

            var affected = 0;

            await RequestSessionsAsync(
                async session =>
                {
                    var contentManager = new ContentManagerMock(session);

                    var centerProfile = await contentManager.NewAsync(ContentTypes.CenterProfile);
                    centerProfile.Alter<CenterProfileManagerExtensionsPart>(p =>
                    {
                        p.RenewalAccreditationStatus = originalAccreditationStatus;
                        p.RenewalCenterProfileStatus = centerProfileStatus;
                    });

                    await contentManager.CreateAsync(centerProfile);
                },
                async session =>
                {
                    var service = new RenewalPeriodService(
                        new CenterProfileServiceMock(session),
                        new CenterProfileTenantServiceMock(session),
                        null,
                        null,
                        null,
                        null,
                        null,
                        new RenewalPeriodSettingsServiceMock(new CenterRenewalSettings()
                        {
                            RenewalPeriods = new[]
                            {
                                new RenewalPeriod() { RenewalStartDate = DateTime.SpecifyKind(DateTime.Parse("2015-08-27 08:00"), DateTimeKind.Utc) }
                            }
                        }));

                    affected = await service.ResetCenterProfileStatusesAsync(DateTime.SpecifyKind(DateTime.Parse("2015-08-27 00:00"), DateTimeKind.Utc));
                },
                async session =>
                {
                    var centerProfile = await session
                        .Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentTypes.CenterProfile)
                        .LatestAndPublished()
                        .FirstOrDefaultAsync();

                    var part = centerProfile.As<CenterProfileManagerExtensionsPart>();

                    if (shouldReset)
                    {
                        Assert.True(!part.RenewalAccreditationStatus.HasValue && part.RenewalCenterProfileStatus == CenterProfileStatus.Unsubmitted);
                        Assert.True(affected > 0);
                    }
                    else
                    {
                        Assert.Equal(part.RenewalAccreditationStatus, originalAccreditationStatus);
                        Assert.Equal(part.RenewalCenterProfileStatus, centerProfileStatus);
                        Assert.True(affected == 0);
                    }
                });
        }

        #endregion


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
                        EmailNotificationSchemaBuilder.Build(schemaBuilder);
                    },
                    store =>
                    {
                        store.RegisterIndexes<ContentItemIndexProvider>();
                        store.RegisterIndexes<CenterProfilePartIndexProvider>();
                        store.RegisterIndexes<CenterProfileManagerExtensionsPartIndexProvider>();
                        store.RegisterIndexes<TerritoryIndexProvider>();
                        store.RegisterIndexes<SettlementIndexProvider>();
                        store.RegisterIndexes<EmailNotificationIndexProvider>();
                    });
                await sessionHandler.RequestSessionsAsync(sessions);
            }
        }


        private class EmailTemplateProviderMock : IEmailTemplateProvider
        {
            private readonly EmailTemplate _expectedResult;


            public EmailTemplateProviderMock(EmailTemplate expectedResult)
            {
                _expectedResult = expectedResult;
            }


            public Task<EmailTemplate> GetEmailTemplateByIdAsync(string id)
                => Task.FromResult(_expectedResult);

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync()
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<string> ProcessAsync(string templateId, object data, string rawBody)
            {
                throw new NotImplementedException();
            }
        }

        private class NonceServiceMock : INonceService
        {
            [ExcludeFromCodeCoverage]
            public Task CleanupAsync()
            {
                throw new NotImplementedException();
            }

            public Task CreateAsync(Nonce nonce) => Task.CompletedTask;

            [ExcludeFromCodeCoverage]
            public Task CreateManyAsync(IEnumerable<Nonce> nonces) => Task.CompletedTask;

            [ExcludeFromCodeCoverage]
            public Task<Nonce> GetByValue(Guid value)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<string> GetUriAsync(Guid nonce)
            {
                throw new NotImplementedException();
            }
        }

        private class RenewalPeriodSettingsServiceMock : IRenewalPeriodSettingsService
        {
            private readonly CenterRenewalSettings _settings;


            public RenewalPeriodSettingsServiceMock(CenterRenewalSettings settings)
            {
                _settings = settings;
            }


            [ExcludeFromCodeCoverage]
            public Task DeleteRenewalSettingsAsync(Guid id)
                => throw new NotImplementedException();

            public Task<CenterRenewalSettings> GetCenterRenewalSettingsAsync()
                => Task.FromResult(_settings);

            [ExcludeFromCodeCoverage]
            public Task<RenewalSettingsViewModel> GetRenewalSettingsAsync(Guid? id = null)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<RenewalSettingsViewModel>> ListRenewalSettingsAsync()
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task MarkProcessedTimingAsync(Guid renewalPeriodId, DateTime currentTiming)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task UpdateRenewalSettingsAsync(RenewalSettingsViewModel viewModel)
                => throw new NotImplementedException();
        }

        private class CenterProfileServiceMock : ICenterProfileService
        {
            private readonly ISession _session;


            public CenterProfileServiceMock(ISession session)
            {
                _session = session;
            }


            public Task<bool> CacheEnabledAsync()
                => Task.FromResult(true);

            public Task<IEnumerable<ContentItem>> GetCenterProfilesAsync()
                => _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentTypes.CenterProfile)
                    .ListAsync();

            public void ClearCenterProfileCache()
            {
            }

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<string>> AcceptManyAsync(IEnumerable<CenterProfileDecisionStateViewModel> viewModelStates)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task CalculateAccreditationStatusAsync(ContentItem contentItem, Action<CenterProfileManagerExtensionsPart> alteration = null)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task ChangeCenterProfileLeaderAsync(ContentItem contentItem, int memberRightId)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task DeleteCenterProfileAsync(string contentItemId)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task DeleteOwnCenterProfileAsync(string contentItemId, int memberRightId)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<ContentItem> GetCenterProfileAsync(string contentItemId)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<CenterProfileEquipmentsSettings> GetCenterProfileEquipmentSettingsAsync()
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<ContentItem>> GetCenterProfilesToLeaderAsync(int memberRightId)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<ContentItem> GetCenterProfileToLeaderAsync(int memberRightId, string contentItemId)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<ContentItem>> GetCenterProfileVersionsAsync(string contentItemId)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<ContentItem> GetPermittedCenterProfileAsync(string id)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<ContentItem>> GetPermittedCenterProfilesAsync()
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<ContentItem>> GetUnsubmittedCenterProfilesAsync(DateTime renewalStartDate)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<ContentItem>> LoadCenterProfilesFromCacheAsync()
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<ContentItem> NewCenterProfileAsync(int memberRightId, bool shouldCreate)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task OnUserProfilesUpdatedAsync(IEnumerable<int> memberRightIds)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task ReviewAsync(CenterProfileReviewCheckResult reviewCheckResult, bool accepted, string comment)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task SaveCenterProfileAsync(ContentItem contentItem, ICenterProfileViewModel viewModel)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task SaveCenterProfileAsync(ContentItem contentItem, bool submitted)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task SetCenterProfileEquipmentSettingsAsync(CenterProfileEquipmentsSettings settings)
            {
                throw new NotImplementedException();
            }
        }

        private class CenterProfileTenantServiceMock : ICenterProfileTenantService
        {
            private readonly ISession _session;


            public CenterProfileTenantServiceMock(ISession session)
            {
                _session = session;
            }


            public Task<ContentItem> RequireCenterProfileContentItemInNewRenewalProcessAsync(ContentItem contentItem, bool shouldEmptyCache)
            {
                contentItem.Alter<CenterProfileManagerExtensionsPart>(p =>
                {
                    p.AccreditationStatusResult = null;
                    p.RenewalAccreditationStatus = null;
                    p.RenewalCenterProfileStatus = CenterProfileStatus.Unsubmitted;
                });

                _session.Save(contentItem);

                return Task.FromResult(contentItem);
            }

            [ExcludeFromCodeCoverage]
            public Task CalculateAccreditationStatusAsync(ContentItem contentItem)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<Colleague> ExecuteColleagueActionAsync(ContentItem contentItem, CenterProfileColleagueActionViewModel viewModel)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<ContentItem> GetCenterProfileAssignedToTenantAsync(string tenantName)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<ContentItem>> GetCenterProfilesForTenantAsync(string tenantName)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<Colleague> InviteColleagueAsync(ContentItem contentItem, CenterProfileColleagueViewModel viewModel)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task SaveCenterProfileAsync(ContentItem contentItem, bool submitted)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task SetCenterProfileAssignmentAsync(string contentItemId, string tenantName)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<ContentItem> UpdateCenterProfileAsync(string tenantName, ICenterProfileViewModel viewModel)
                => throw new NotImplementedException();
        }
    }
}
