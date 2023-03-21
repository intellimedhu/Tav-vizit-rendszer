using IntelliMed.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    public abstract class PeriodCounterPartOrganizationDisplayDriver<TPart> : PeriodCounterPartDisplayDriver<TPart>
        where TPart : ContentPart, new()
    {
        protected readonly ISharedDataAccessorService _sharedDataAccessorService;


        public PeriodCounterPartOrganizationDisplayDriver(IClock clock, ISharedDataAccessorService sharedDataAccessorService)
            : base(clock)
        {
            _sharedDataAccessorService = sharedDataAccessorService;
        }


        protected override async Task<RenewalPeriod> GetPeriodAsync()
        {
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                var renewalPeriodSettingsService = scope.ServiceProvider.GetRequiredService<IRenewalPeriodSettingsService>();

                var renewalSettings = await renewalPeriodSettingsService.GetCenterRenewalSettingsAsync();

                return renewalSettings.GetCurrentFullPeriod(_clock.UtcNow);
            }
        }
    }
}
