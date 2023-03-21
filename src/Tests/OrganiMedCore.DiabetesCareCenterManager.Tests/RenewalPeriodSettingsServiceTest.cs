using IntelliMed.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class RenewalPeriodSettingsServiceTest
    {
        [Fact]
        public async Task Add_ShouldBeAdded()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            var renewalStartDate = new DateTime(2018, 1, 1, 7, 0, 0);

            await service.UpdateRenewalSettingsAsync(new RenewalSettingsViewModel()
            {
                EmailTimings = new List<DateTime>()
                {
                    renewalStartDate.AddDays(10).AddHours(11),
                    renewalStartDate.AddDays(5).AddHours(19.5)
                },
                RenewalStartDate = renewalStartDate,
                ReviewStartDate = renewalStartDate.AddDays(30),
                ReviewEndDate = renewalStartDate.AddDays(40)
            });

            var list = (await service.ListRenewalSettingsAsync()).ToArray();

            Assert.Single(list);
            Assert.True(list.First().Id != Guid.Empty);
        }

        [Fact]
        public async Task List_ShouldReturnAll()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            await service.UpdateRenewalSettingsAsync(new RenewalSettingsViewModel()
            {
                EmailTimings = new List<DateTime>()
                {
                    DateTime.Now.Date.AddDays(7).AddHours(11),
                    DateTime.Now.Date.AddDays(20).AddHours(2.025)
                },
                RenewalStartDate = DateTime.Now.Date.AddMinutes(9805),
                ReviewStartDate = DateTime.Now.AddMonths(1),
                ReviewEndDate = DateTime.Now.AddMonths(1).AddDays(7)
            });

            var list = (await service.ListRenewalSettingsAsync()).ToArray();

            Assert.NotEmpty(list);
            Assert.Single(list);
        }

        [Fact]
        public async Task Get_ShouldReturnOne()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            await service.UpdateRenewalSettingsAsync(new RenewalSettingsViewModel()
            {
                EmailTimings = new List<DateTime>()
                {
                    DateTime.Now.Date.AddDays(7).AddHours(1.5)
                },
                RenewalStartDate = DateTime.Now.Date.Add(new TimeSpan(3, 43, 22)),
                ReviewStartDate = DateTime.Now.Date.AddDays(19),
                ReviewEndDate = DateTime.Now.Date.AddDays(27)
            });

            var id = (await service.ListRenewalSettingsAsync()).First().Id;

            Assert.NotNull(await service.GetRenewalSettingsAsync(id));
        }

        [Fact]
        public async Task Get_ShouldThrownNotFoundException()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetRenewalSettingsAsync(Guid.Empty));
        }

        [Fact]
        public async Task Get_ShouldReturnNew()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            var viewModel = await service.GetRenewalSettingsAsync();

            Assert.NotNull(viewModel);
            Assert.Null(viewModel.Id);
            Assert.Null(viewModel.RenewalStartDate);
            Assert.Null(viewModel.ReviewStartDate);
            Assert.Empty(viewModel.EmailTimings);
            Assert.Empty(viewModel.ProcessedTimings);
        }

        [Fact]
        public async Task Update_ShouldBeUpdated()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            var renewalStartDate = DateTime.Now.AddDays(-12).Date;
            var renewalStartTime = TimeSpan.FromHours(7.32091117);

            await service.UpdateRenewalSettingsAsync(new RenewalSettingsViewModel()
            {
                EmailTimings = new List<DateTime>()
                {
                    //new EmailTiming() { Date = renewalStartDate.AddDays(3), Time = TimeSpan.FromHours(10) },
                    //new EmailTiming() { Date = renewalStartDate.AddDays(4), Time = TimeSpan.FromHours(9.5) },
                    //new EmailTiming() { Date = renewalStartDate.AddDays(5), Time = TimeSpan.FromHours(7) },
                    //new EmailTiming() { Date = renewalStartDate.AddDays(6), Time = TimeSpan.FromHours(4.72) }
                },
                RenewalStartDate = renewalStartDate.Add(renewalStartTime),
                ReviewStartDate = renewalStartDate.AddDays(41),
                ReviewEndDate = DateTime.Now.AddMonths(2)
            });

            var viewModel = (await service.ListRenewalSettingsAsync()).First();

            await service.UpdateRenewalSettingsAsync(viewModel);

            var list = await service.ListRenewalSettingsAsync();

            Assert.Single(list);
        }

        [Fact]
        public async Task Update_ShouldThrownNotFoundException()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateRenewalSettingsAsync(new RenewalSettingsViewModel()
                {
                    Id = Guid.Empty,
                    RenewalStartDate = DateTime.Parse("2019-01-01").Add(TimeSpan.Parse("05:05")),
                    ReviewStartDate = DateTime.Parse("2019-02-15").Add(TimeSpan.Parse("23:00")),
                    ReviewEndDate = DateTime.Parse("2019-02-25"),
                }));
        }

        [Fact]
        public async Task Update_ShouldThrownRenewalTimingOutOfDateRangeException()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            await Assert.ThrowsAsync<RenewalTimingOutOfDateRangeException>(() =>
                service.UpdateRenewalSettingsAsync(new RenewalSettingsViewModel()
                {
                    Id = Guid.Empty,
                    RenewalStartDate = DateTime.Parse("2019-01-01").Add(TimeSpan.Parse("05:05")),
                    ReviewStartDate = DateTime.Parse("2019-02-15").Add(TimeSpan.Parse("23:00")),
                    ReviewEndDate = DateTime.Parse("2019-02-26").Add(TimeSpan.Parse("11:12")),
                    EmailTimings = new[]
                    {
                        DateTime.Parse("2019-02-15").Add(TimeSpan.Parse("23:23"))
                    }
                }));
        }

        [Theory]
        [InlineData("1988-05-02", "1988-05-01", "1988-05-03")]
        [InlineData("1988-05-02", "1988-05-04", "1988-05-01")]
        [InlineData("1988-05-02", "1988-05-04", "1988-05-03")]        
        public async Task Update_ShouldThrownRenewalTimingException(DateTime renewalStartDate, DateTime reviewStartDate, DateTime reviewEndDate)
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            await Assert.ThrowsAsync<RenewalTimingException>(() =>
                service.UpdateRenewalSettingsAsync(new RenewalSettingsViewModel()
                {
                    Id = Guid.Empty,
                    RenewalStartDate = renewalStartDate,
                    ReviewStartDate = reviewStartDate,
                    ReviewEndDate = reviewEndDate
                }));
        }

        [Fact]
        public async Task Delete_ShouldBeDeleted()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            var renewalStartDate = new DateTime(2019, 1, 1);
            var renewalStartTime = TimeSpan.FromHours(7.32091117);

            await service.UpdateRenewalSettingsAsync(new RenewalSettingsViewModel()
            {
                EmailTimings = new List<DateTime>()
                {
                    renewalStartDate.AddDays(3).AddHours(3.34)
                },
                RenewalStartDate = renewalStartDate.Add(renewalStartTime),
                ReviewStartDate = renewalStartDate.AddDays(10),
                ReviewEndDate = renewalStartDate.AddDays(12)
            });

            var viewModel = (await service.ListRenewalSettingsAsync()).First();

            await service.DeleteRenewalSettingsAsync(viewModel.Id.Value);

            var list = await service.ListRenewalSettingsAsync();

            Assert.Empty(list);
        }

        [Fact]
        public async Task Delete_ShouldThrownNotFoundException()
        {
            var service = new RenewalPeriodSettingsService(new SiteServiceMock());

            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteRenewalSettingsAsync(Guid.Empty));
        }
    }
}
