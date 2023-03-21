using IntelliMed.Core.Services;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.Organization.DiabetesCareCenter.Widgets")]
    public class RenewalPeriodCounterPartOrganizationDisplayDriver : PeriodCounterPartOrganizationDisplayDriver<RenewalPeriodCounterPart>
    {
        protected override string DisplayedViewName => "RenewalPeriodCounter";

        protected override string HtmlPrefix => "renewal";

        protected override bool FullPeriod => true;


        public RenewalPeriodCounterPartOrganizationDisplayDriver(IClock clock, ISharedDataAccessorService sharedDataAccessorService)
            : base(clock, sharedDataAccessorService)
        {
        }
    }
}
