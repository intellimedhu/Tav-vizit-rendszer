using System;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels
{
    public class RenewalPeriodViewModel
    {
        public bool IsPeriod { get; set; }

        public string HtmlPrefix { get; set; }

        public DateTime PeriodStartDateUtc { get; set; }

        public DateTime PeriodEndDateUtc { get; set; }
    }
}
