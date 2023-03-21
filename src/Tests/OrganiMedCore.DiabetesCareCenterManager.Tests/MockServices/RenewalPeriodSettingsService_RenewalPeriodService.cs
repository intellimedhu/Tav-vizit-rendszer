using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Tests.MockServices
{
    public class RenewalPeriodSettingsService_RenewalPeriodService : IRenewalPeriodSettingsService
    {
        private readonly CenterRenewalSettings _centerRenewalSettings;


        public RenewalPeriodSettingsService_RenewalPeriodService(CenterRenewalSettings centerRenewalSettings)
        {
            _centerRenewalSettings = centerRenewalSettings;
        }


        [ExcludeFromCodeCoverage]
        public Task DeleteRenewalSettingsAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<CenterRenewalSettings> GetCenterRenewalSettingsAsync()
            => Task.FromResult(_centerRenewalSettings);

        [ExcludeFromCodeCoverage]
        public Task<RenewalSettingsViewModel> GetRenewalSettingsAsync(Guid? id = null)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<RenewalSettingsViewModel>> ListRenewalSettingsAsync()
        {
            throw new NotImplementedException();
        }

        public Task MarkProcessedTimingAsync(Guid renewalPeriodId, DateTime currentTiming)
        {
            var period = _centerRenewalSettings.RenewalPeriods.FirstOrDefault(x => x.Id == renewalPeriodId);
            if (period != null && period.EmailTimings.Contains(currentTiming))
            {
                period.ProcessedTimings.Add(currentTiming);
            }

            return Task.CompletedTask;
        }

        [ExcludeFromCodeCoverage]
        public Task UpdateRenewalSettingsAsync(RenewalSettingsViewModel viewModel)
        {
            throw new NotImplementedException();
        }
    }
}
