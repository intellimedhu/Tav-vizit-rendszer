using Fluid;
using Fluid.Values;
using IntelliMed.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell.Builders;
using OrganiMedCore.DiabetesCareCenter.Core.LiquidFilters;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests
{
    public class FullRenewalPeriodFilterTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid-date")]
        public async Task Process_ShouldReturn_False(string dateUtc)
        {
            var filter = new FullRenewalPeriodFilter(new ServiceProviderMock(null));
            var resultValue = await filter.ProcessAsync(FluidValue.Create(dateUtc), null, null);
            Assert.False(resultValue.ToBooleanValue());
        }

        [Theory]
        // From manager
        [InlineData("1977-10-10 14:29:59", false, false)]
        [InlineData("1977-10-10 14:30", false, true)]
        [InlineData("1977-10-15 22:59", false, true)]
        [InlineData("1977-10-15 23:00", false, true)]
        [InlineData("1977-10-17 18:14:59", false, true)]
        [InlineData("1977-10-17 18:15", false, false)]
        // From tenant
        [InlineData("1977-10-10 14:29:59", true, false)]
        [InlineData("1977-10-10 14:30", true, true)]
        [InlineData("1977-10-15 22:59", true, true)]
        [InlineData("1977-10-15 23:00", true, true)]
        [InlineData("1977-10-17 18:14:59", true, true)]
        [InlineData("1977-10-17 18:15", true, false)]
        public async Task Process_ShouldReturn_AsExpected(string dateUtc, bool isTenant, bool expectedValue)
        {
            var dateLocal = DateTime.SpecifyKind(DateTime.Parse(dateUtc), DateTimeKind.Utc).ToLocalTime();

            var settings = new CenterRenewalSettings()
            {
                RenewalPeriods = new List<RenewalPeriod>()
                {
                    new RenewalPeriod()
                    {
                        RenewalStartDate = DateTime.SpecifyKind(DateTime.Parse("1977-10-10 14:30"), DateTimeKind.Utc),
                        ReviewStartDate = DateTime.SpecifyKind(DateTime.Parse("1977-10-15 23:00"), DateTimeKind.Utc),
                        ReviewEndDate = DateTime.SpecifyKind(DateTime.Parse("1977-10-17 18:15"), DateTimeKind.Utc)
                    }
                }
            };
            var filter = new FullRenewalPeriodFilter(new ServiceProviderMock(settings));
            var resultValue = await filter.ProcessAsync(FluidValue.Create(dateLocal), new FilterArguments(isTenant), null);

            Assert.Equal(expectedValue, resultValue.ToBooleanValue());
        }


        private class ServiceProviderMock : IServiceProvider
        {
            private readonly CenterRenewalSettings _expectedResult;


            public ServiceProviderMock(CenterRenewalSettings expectedResult)
            {
                _expectedResult = expectedResult;
            }


            [ExcludeFromCodeCoverage]
            public object GetService(Type serviceType)
            {
                if (serviceType.IsAssignableFrom(typeof(IRenewalPeriodSettingsService)))
                {
                    return new RenewalPeriodSettingsServiceMock(_expectedResult);
                }

                if (serviceType.IsAssignableFrom(typeof(ISharedDataAccessorService)))
                {
                    return new SharedDataAccessorServiceMock(_expectedResult);
                }

                throw new ArgumentException();
            }
        }

        private class RenewalPeriodSettingsServiceMock : IRenewalPeriodSettingsService
        {
            private readonly CenterRenewalSettings _expectedResult;


            public RenewalPeriodSettingsServiceMock(CenterRenewalSettings expectedResult)
            {
                _expectedResult = expectedResult;
            }


            public Task<CenterRenewalSettings> GetCenterRenewalSettingsAsync()
                => Task.FromResult(_expectedResult);

            [ExcludeFromCodeCoverage]
            public Task DeleteRenewalSettingsAsync(Guid id)
                => throw new NotImplementedException();

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

        private class SharedDataAccessorServiceMock : ISharedDataAccessorService
        {
            private readonly CenterRenewalSettings _expectedResult;


            public SharedDataAccessorServiceMock(CenterRenewalSettings expectedResult)
            {
                _expectedResult = expectedResult;
            }


            public Task<IServiceScope> GetTenantServiceScopeAsync(string tenantName)
                => Task.FromResult((IServiceScope)new ServiceScopeMock(_expectedResult));

            [ExcludeFromCodeCoverage]
            public Task<IShape> BuildManagerEditorAsync(IServiceScope managersServiceScope, ContentItem content, IUpdateModel updater, bool isNew, string groupId = "", string htmlFieldPrefix = "")
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<T> GetAsync<T>(IServiceScope managersServiceScope, int id) where T : class
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<IServiceScope> GetManagerServiceScopeAsync()
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<IEnumerable<ShellContext>> ListShellContextsAsync()
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public IQuery<T> Query<T>(IServiceScope managersServiceScope) where T : class
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public void Save(IServiceScope managersServiceScope, object obj)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            public Task<IShape> UpdateManagerEditorAsync(IServiceScope managersServiceScope, ContentItem content, IUpdateModel updater, bool isNew, string groupId = "", string htmlFieldPrefix = "")
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            IQuery<T, TIndex> ISharedDataAccessorService.Query<T, TIndex>(IServiceScope managersServiceScope, bool filterType)
            {
                throw new NotImplementedException();
            }

            [ExcludeFromCodeCoverage]
            IQuery<T, TIndex> ISharedDataAccessorService.Query<T, TIndex>(IServiceScope managersServiceScope, Expression<Func<TIndex, bool>> predicate, bool filterType)
            {
                throw new NotImplementedException();
            }
        }

        private class ServiceScopeMock : IServiceScope
        {
            private readonly CenterRenewalSettings _expectedResult;


            public IServiceProvider ServiceProvider => new ServiceProviderMock(_expectedResult);


            public ServiceScopeMock(CenterRenewalSettings expectedResult)
            {
                _expectedResult = expectedResult;
            }


            public void Dispose()
            {
            }
        }
    }
}
