using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class RenewalPeriodCounterPartManagerDisplayDriver : PeriodCounterPartManagerDisplayDriver<RenewalPeriodCounterPart>
    {
        protected override string DisplayedViewName => "RenewalPeriodCounter";

        protected override string HtmlPrefix => "renewal";

        protected override bool FullPeriod => true;


        public RenewalPeriodCounterPartManagerDisplayDriver(IClock clock, IRenewalPeriodSettingsService renewalPeriodSettingsService)
            : base(clock, renewalPeriodSettingsService)
        {
        }
    }
}
