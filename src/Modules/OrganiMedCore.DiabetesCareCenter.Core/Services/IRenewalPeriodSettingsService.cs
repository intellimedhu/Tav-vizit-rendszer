using IntelliMed.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Services
{
    public interface IRenewalPeriodSettingsService
    {
        /// <exception cref="NotFoundException"></exception>
        Task DeleteRenewalSettingsAsync(Guid id);

        /// <exception cref="NotFoundException"></exception>
        Task<RenewalSettingsViewModel> GetRenewalSettingsAsync(Guid? id = null);

        Task<IEnumerable<RenewalSettingsViewModel>> ListRenewalSettingsAsync();

        Task<CenterRenewalSettings> GetCenterRenewalSettingsAsync();

        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="RenewalTimingOutOfDateRangeException"></exception>
        Task UpdateRenewalSettingsAsync(RenewalSettingsViewModel viewModel);

        Task MarkProcessedTimingAsync(Guid renewalPeriodId, DateTime currentTiming);
    }
}
