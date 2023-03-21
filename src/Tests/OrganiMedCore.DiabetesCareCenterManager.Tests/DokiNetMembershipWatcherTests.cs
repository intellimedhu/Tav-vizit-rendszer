using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class DokiNetMembershipWatcherTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task DoWorkAsync_InRenewalPeriod(bool shouldThrowException)
        {
            var utcNow = DateTime.Parse("1969-06-09 09:06");

            var lastCheckDate = utcNow.AddMinutes(7);

            var siteService = new SiteServiceMock();
            var site = await siteService.GetSiteSettingsAsync();
            var logs = site.Alter<DokiNetMembershipWatcherLog>(nameof(DokiNetMembershipWatcherLog), log =>
            {
                log.LastCheckDate = utcNow.AddMinutes(-5);
            });
            site.Alter<CenterRenewalSettings>(nameof(CenterRenewalSettings), x =>
            {
                x.RenewalPeriods.Add(new RenewalPeriod()
                {
                    RenewalStartDate = utcNow.AddDays(-10).Date,
                    ReviewEndDate = utcNow.AddDays(10).Date
                });
            });

            await siteService.UpdateSiteSettingsAsync(site);

            var centerProfileService = new CpsMock(shouldThrowException);
            var logger = new LoggerMock<DokiNetMembershipWatcher>();

            var sp = new SpMock(
                centerProfileService,
                new ClockMock(utcNow),
                new DnMock(lastCheckDate, new[] { 100, 200, 300, 400 }),
                logger,
                siteService);

            var service = new DokiNetMembershipWatcher();
            await service.DoWorkAsync(sp, CancellationToken.None);

            if (!shouldThrowException)
            {
                // TODO: better mock DokiNetService to test whether response hasn't any MemberRightIds.
                // 100, 200, 300, 400
                Assert.Contains(centerProfileService.MemberRightIds, x => x == 100);
                Assert.Contains(centerProfileService.MemberRightIds, x => x == 200);
                Assert.Contains(centerProfileService.MemberRightIds, x => x == 300);
                Assert.Contains(centerProfileService.MemberRightIds, x => x == 400);

                Assert.Contains(logger.Logs, x => x.State.ToString().StartsWith("Center profiles updated"));
                Assert.Contains(logger.Logs, x => x.State.ToString().StartsWith("Updating LastCheckDate"));

                var watchLog = (await siteService.GetSiteSettingsAsync()).As<DokiNetMembershipWatcherLog>();
                Assert.Equal(lastCheckDate, watchLog.LastCheckDate);
            }
            else
            {
                Assert.Empty(centerProfileService.MemberRightIds);
                Assert.NotEmpty(logger.Logs);
            }
        }

        [Fact]
        public async Task DoWorkAsync_InRenewalPeriod_NoUpdatedMembers()
        {
            var utcNow = DateTime.Parse("1969-06-09 09:06");

            var lastCheckDate = utcNow.AddMinutes(7);

            var siteService = new SiteServiceMock();
            var site = await siteService.GetSiteSettingsAsync();
            var logs = site.Alter<DokiNetMembershipWatcherLog>(nameof(DokiNetMembershipWatcherLog), log =>
            {
                log.LastCheckDate = utcNow.AddMinutes(-5);
            });
            site.Alter<CenterRenewalSettings>(nameof(CenterRenewalSettings), x =>
            {
                x.RenewalPeriods.Add(new RenewalPeriod()
                {
                    RenewalStartDate = utcNow.AddDays(-10).Date,
                    ReviewEndDate = utcNow.AddDays(10).Date
                });
            });

            await siteService.UpdateSiteSettingsAsync(site);

            var logger = new LoggerMock<DokiNetMembershipWatcher>();

            var sp = new SpMock(
                null,
                new ClockMock(utcNow),
                new DnMock(lastCheckDate, new int[0]),
                logger,
                siteService);

            var service = new DokiNetMembershipWatcher();
            await service.DoWorkAsync(sp, CancellationToken.None);

            Assert.Contains(logger.Logs, l => l.State.ToString() == "No updated members.");
        }

        [Theory]
        [InlineData("2019-09-09 00:00:00", false)]
        [InlineData("2019-09-09 00:06:59", false)]
        [InlineData("2019-09-09 00:07:00", true)]
        [InlineData("2019-09-10 00:00", true)]
        public async Task DoWorkAsync_OutOfRenewalPeriod(string utcNowStr, bool shouldIncludeOtherLog)
        {
            var utcNow = DateTime.Parse(utcNowStr);

            var lastCheckDate = utcNow.AddMinutes(-4);

            var siteService = new SiteServiceMock();
            var site = await siteService.GetSiteSettingsAsync();
            var logs = site.Alter<DokiNetMembershipWatcherLog>(nameof(DokiNetMembershipWatcherLog), log =>
            {
                log.LastCheckDate = lastCheckDate;
            });
            site.Alter<CenterRenewalSettings>(nameof(CenterRenewalSettings), x => { });

            await siteService.UpdateSiteSettingsAsync(site);

            var logger = new LoggerMock<DokiNetMembershipWatcher>();

            await new DokiNetMembershipWatcher().DoWorkAsync(
                new SpMock(null, new ClockMock(utcNow), new DnMock(lastCheckDate, null), logger, siteService),
                CancellationToken.None);

            Assert.Contains(logger.Logs, x => x.State.ToString() == "No renewal period.");

            if (shouldIncludeOtherLog)
            {
                Assert.Contains(logger.Logs, x => x.State.ToString() == "No week start.");
            }
            else
            {
                Assert.DoesNotContain(logger.Logs, x => x.State.ToString() == "No week start.");
            }
        }

        [Fact]
        public async Task DoWorkAsync_NoRenewalPeriod_NoWeekStart()
        {
            var utcNow = DateTime.Parse("2001-01-23 14:14");
            var lastCheckDate = utcNow.AddMinutes(7);

            var siteService = new SiteServiceMock();
            var site = await siteService.GetSiteSettingsAsync();
            var logs = site.Alter<DokiNetMembershipWatcherLog>(nameof(DokiNetMembershipWatcherLog), log =>
            {
                log.LastCheckDate = utcNow.AddMinutes(-5);
            });
            site.Alter<CenterRenewalSettings>(nameof(CenterRenewalSettings), x => { });

            await siteService.UpdateSiteSettingsAsync(site);

            var logger = new LoggerMock<DokiNetMembershipWatcher>();

            var watcher = new DokiNetMembershipWatcher();
            await watcher.DoWorkAsync(
                new SpMock(null, new ClockMock(utcNow), null, logger, siteService),
                CancellationToken.None);

            Assert.Contains(logger.Logs, x => x.State.ToString() == "No renewal period.");
            Assert.Contains(logger.Logs, x => x.State.ToString() == "No week start.");
        }


        private class SpMock : IServiceProvider
        {
            private readonly ICenterProfileService _centerProfileService;
            private readonly IDokiNetService _dokiNetService;
            private readonly IClock _clock;
            private readonly LoggerMock<DokiNetMembershipWatcher> _logger;
            private readonly ISiteService _siteService;


            public SpMock(
                ICenterProfileService centerProfileService,
                IClock clock,
                IDokiNetService dokiNetService,
                LoggerMock<DokiNetMembershipWatcher> logger,
                ISiteService siteService)
            {
                _centerProfileService = centerProfileService;
                _clock = clock;
                _dokiNetService = dokiNetService;
                _logger = logger;
                _siteService = siteService;
            }



            public object GetService(Type serviceType)
            {
                if (serviceType.Name.Contains("ILogger"))
                {
                    return _logger;
                }

                return serviceType.Name switch
                {
                    "IDokiNetService" => _dokiNetService,
                    "ISiteService" => _siteService,
                    "ICenterProfileService" => _centerProfileService,
                    "IClock" => _clock,
                    "IRenewalPeriodSettingsService" => new RenewalPeriodSettingsService(_siteService),
                    _ => null,
                };
            }
        }

        private class CpsMock : ICenterProfileService
        {
            private readonly bool _shouldThrowException;


            public IEnumerable<int> MemberRightIds { get; private set; } = new List<int>();


            public CpsMock(bool shouldThrowException)
            {
                _shouldThrowException = shouldThrowException;
            }


            public Task OnUserProfilesUpdatedAsync(IEnumerable<int> memberRightIds)
            {
                if (!_shouldThrowException)
                {
                    MemberRightIds = memberRightIds;

                    return Task.CompletedTask;
                }

                throw new Exception();
            }

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<string>> AcceptManyAsync(IEnumerable<CenterProfileDecisionStateViewModel> viewModelStates)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<bool> CacheEnabledAsync()
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
            public void ClearCenterProfileCache()
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
            public Task<IEnumerable<ContentItem>> GetCenterProfilesAsync()
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

            public Task<ContentItem> NewCenterProfileAsync(int memberRightId, bool shouldCreate)
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

        private class DnMock : IDokiNetService
        {
            private readonly DateTime _lastCheckDate;
            private readonly int[] _memberRightIds;


            public DnMock(DateTime lastCheckDate, int[] memberRightIds)
            {
                _lastCheckDate = lastCheckDate;
                _memberRightIds = memberRightIds;
            }


            public Task<MembershipWatchResponse> WatchMembershipAsync(DateTime lastCheckDateUtc)
                => Task.FromResult(new MembershipWatchResponse()
                {
                    MemberRightIds = _memberRightIds,
                    LastCheckDate = _lastCheckDate
                });

            [ExcludeFromCodeCoverage]
            public Task<T> GetDokiNetMemberById<T>(int memberRightId) where T : DokiNetMember
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<T> GetDokiNetMemberByLoginAsync<T>(string username, string password) where T : DokiNetMember
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<T> GetDokiNetMemberByNonce<T>(string nonce) where T : DokiNetMember
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<T>> GetDokiNetMembersByIds<T>(IEnumerable<int> memberRightIds) where T : DokiNetMember
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task SaveMemberDataAnsyc(int memberId, IEnumerable<MemberData> values)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<T>> SearchDokiNetMemberByName<T>(string name) where T : DokiNetMember
                => throw new NotImplementedException();
        }
    }
}
