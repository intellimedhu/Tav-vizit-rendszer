using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class UpdatePeriodCounterPartManagerDisplayDriver : PeriodCounterPartManagerDisplayDriver<UpdatePeriodCounterPart>
    {
        protected override string DisplayedViewName => "UpdatePeriodCounter";

        protected override string HtmlPrefix => "update";

        protected override bool FullPeriod => false;


        public UpdatePeriodCounterPartManagerDisplayDriver(IClock clock, IRenewalPeriodSettingsService renewalPeriodSettingsService)
            : base(clock, renewalPeriodSettingsService)
        {
        }
    }
}
