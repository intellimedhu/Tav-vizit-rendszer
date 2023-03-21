using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Extensions;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Exceptions;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class RenewalPeriodSettingsService : IRenewalPeriodSettingsService
    {
        private readonly ISiteService _siteService;


        public RenewalPeriodSettingsService(ISiteService siteService)
        {
            _siteService = siteService;
        }


        public async Task DeleteRenewalSettingsAsync(Guid id)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            if (siteSettings.As<CenterRenewalSettings>().RenewalPeriods.SingleOrDefault(x => x.Id == id) == null)
            {
                throw new NotFoundException();
            }

            siteSettings.Alter<CenterRenewalSettings>(nameof(CenterRenewalSettings), renewalSettings =>
            {
                renewalSettings.RenewalPeriods = renewalSettings.RenewalPeriods.Where(x => x.Id != id).ToList();
            });

            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }

        public async Task<RenewalSettingsViewModel> GetRenewalSettingsAsync(Guid? id = null)
        {
            var renewalSettings = (await _siteService.GetSiteSettingsAsync()).As<CenterRenewalSettings>();

            var viewModel = new RenewalSettingsViewModel();
            if (id != null)
            {
                var renewalPeriod = renewalSettings.RenewalPeriods.FirstOrDefault(x => x.Id == id);
                if (renewalPeriod == null)
                {
                    throw new NotFoundException();
                }

                viewModel.UpdateViewModel(renewalPeriod);
            }

            return viewModel;
        }

        public async Task<CenterRenewalSettings> GetCenterRenewalSettingsAsync()
            => (await _siteService.GetSiteSettingsAsync()).As<CenterRenewalSettings>();

        public async Task<IEnumerable<RenewalSettingsViewModel>> ListRenewalSettingsAsync()
            => (await GetCenterRenewalSettingsAsync())
                .RenewalPeriods.Select(x =>
                {
                    var viewModel = new RenewalSettingsViewModel();
                    viewModel.UpdateViewModel(x);

                    return viewModel;
                });

        public async Task UpdateRenewalSettingsAsync(RenewalSettingsViewModel viewModel)
        {
            ThrowIfEmpty(viewModel);

            var renewalStartDate = viewModel.RenewalStartDate.Value;
            var reviewStartDate = viewModel.ReviewStartDate.Value;
            var reviewEndDate = viewModel.ReviewEndDate.Value;

            if (renewalStartDate >= reviewStartDate || reviewStartDate >= reviewEndDate)
            {
                throw new RenewalTimingException();
            }

            if (!viewModel.EmailTimings.All(x => x >= renewalStartDate && x <= reviewStartDate))
            {
                throw new RenewalTimingOutOfDateRangeException();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            var isNew = !viewModel.Id.HasValue;
            RenewalPeriod renewalPeriod;
            if (isNew)
            {
                renewalPeriod = new RenewalPeriod()
                {
                    Id = Guid.NewGuid()
                };
            }
            else
            {
                renewalPeriod = siteSettings.As<CenterRenewalSettings>().RenewalPeriods.FirstOrDefault(x => x.Id == viewModel.Id.Value);
                if (renewalPeriod == null)
                {
                    throw new NotFoundException();
                }
            }

            viewModel.UpdateModel(renewalPeriod);

            siteSettings.Alter<CenterRenewalSettings>(nameof(CenterRenewalSettings), renewalSettings =>
            {
                if (!isNew)
                {
                    renewalSettings.RenewalPeriods = renewalSettings.RenewalPeriods.Where(x => x.Id != renewalPeriod.Id).ToList();
                }

                renewalSettings.RenewalPeriods.Add(renewalPeriod);
            });

            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }

        public async Task MarkProcessedTimingAsync(Guid renewalPeriodId, DateTime currentTiming)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            siteSettings.Alter<CenterRenewalSettings>(nameof(CenterRenewalSettings), renewalSettings =>
            {
                var renewalPeriod = renewalSettings.RenewalPeriods.FirstOrDefault(x => x.Id == renewalPeriodId);
                if (renewalPeriod == null)
                {
                    return;
                }

                if (!renewalPeriod.EmailTimings.Any(x => x == currentTiming))
                {
                    return;
                }

                renewalPeriod.ProcessedTimings.Add(currentTiming);
            });

            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }


        private void ThrowIfEmpty(RenewalSettingsViewModel viewModel)
        {
            viewModel.ThrowIfNull();
            viewModel.RenewalStartDate.ThrowIfNull();
            viewModel.ReviewStartDate.ThrowIfNull();
            viewModel.EmailTimings.ThrowIfNull();
        }
    }
}
