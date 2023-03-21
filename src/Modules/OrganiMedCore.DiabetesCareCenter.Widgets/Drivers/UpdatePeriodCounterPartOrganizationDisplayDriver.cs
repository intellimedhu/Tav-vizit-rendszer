using IntelliMed.Core.Services;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.Organization.DiabetesCareCenter.Widgets")]
    public class UpdatePeriodCounterPartOrganizationDisplayDriver : PeriodCounterPartOrganizationDisplayDriver<UpdatePeriodCounterPart>
    {
        protected override string DisplayedViewName => "UpdatePeriodCounter";

        protected override string HtmlPrefix => "update";

        protected override bool FullPeriod => false;


        public UpdatePeriodCounterPartOrganizationDisplayDriver(IClock clock, ISharedDataAccessorService sharedDataAccessorService)
            : base(clock, sharedDataAccessorService)
        {
        }
    }
}
