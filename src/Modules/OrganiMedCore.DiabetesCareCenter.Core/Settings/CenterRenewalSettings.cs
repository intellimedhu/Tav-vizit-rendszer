using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.Settings
{
    public class CenterRenewalSettings
    {
        public IList<RenewalPeriod> RenewalPeriods { get; set; } = new List<RenewalPeriod>();

        [JsonIgnore]
        public RenewalPeriod LatestFullPeriod
        {
            get => RenewalPeriods.OrderByDescending(x => x.RenewalStartDate).FirstOrDefault();
        }

        /// <summary>
        /// Renewal period where the given date is between the period's renewal start date and review start date.
        /// If the given date is later than the review start date of the period the result will be null
        /// even if the review end date of the period is later then the given date.
        /// </summary>
        public RenewalPeriod this[DateTime utcNow]
        {
            get => RenewalPeriods.FirstOrDefault(period =>
                period.RenewalStartDate <= utcNow &&
                utcNow < period.ReviewStartDate);
        }


        public RenewalPeriod GetCurrentFullPeriod(DateTime utcNow)
            => RenewalPeriods.FirstOrDefault(period =>
                period.RenewalStartDate <= utcNow &&
                utcNow < period.ReviewEndDate);

        public RenewalPeriod GetPreviousFullPeriod(DateTime utcNow)
        {
            var periods = RenewalPeriods
                .Where(x => x.ReviewEndDate < utcNow)
                .OrderByDescending(x => x.RenewalStartDate)
                .ToArray();

            if (!periods.Any())
            {
                return null;
            }

            var i = 0;
            while (i < periods.Length && periods[i].ReviewEndDate > utcNow)
            {
                i++;
            }

            return i < periods.Length ? periods[i] : null;
        }
    }
}
