using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    public abstract class PeriodCounterPartDisplayDriver<TPart> : ContentPartDisplayDriver<TPart>
        where TPart : ContentPart, new()
    {
        protected readonly IClock _clock;


        protected abstract string DisplayedViewName { get; }

        protected abstract string HtmlPrefix { get; }

        protected abstract bool FullPeriod { get; }


        public PeriodCounterPartDisplayDriver(IClock clock)
        {
            _clock = clock;
        }


        public override async Task<IDisplayResult> DisplayAsync(TPart part, BuildPartDisplayContext context)
        {
            var period = await GetPeriodAsync();

            return Initialize<RenewalPeriodViewModel>(DisplayedViewName, viewModel =>
                {
                    if (period != null)
                    {
                        viewModel.IsPeriod = true;
                        viewModel.HtmlPrefix = HtmlPrefix;
                        viewModel.PeriodStartDateUtc = period.RenewalStartDate;
                        viewModel.PeriodEndDateUtc = FullPeriod ? period.ReviewEndDate : period.ReviewStartDate;
                    }
                })
                .Location("Detail", "Content:5");
        }


        protected abstract Task<RenewalPeriod> GetPeriodAsync();
    }
}
