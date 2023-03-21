using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    public abstract class PeriodCounterPartManagerDisplayDriver<TPart> : PeriodCounterPartDisplayDriver<TPart>
        where TPart : ContentPart, new()
    {
        protected readonly IRenewalPeriodSettingsService _renewalPeriodSettingsService;


        public PeriodCounterPartManagerDisplayDriver(IClock clock, IRenewalPeriodSettingsService renewalPeriodSettingsService)
            : base(clock)
        {
            _renewalPeriodSettingsService = renewalPeriodSettingsService;
        }


        protected override async Task<RenewalPeriod> GetPeriodAsync()
        {
            var settings = await _renewalPeriodSettingsService.GetCenterRenewalSettingsAsync();

            return settings?.GetCurrentFullPeriod(_clock.UtcNow);
        }
    }
}
