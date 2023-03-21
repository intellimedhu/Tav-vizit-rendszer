using System;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.Settings
{
    public class RenewalPeriod
    {
        public Guid Id { get; set; }

        /// <summary>
        /// UTC date
        /// </summary>
        public DateTime RenewalStartDate { get; set; }

        /// <summary>
        /// UTC date
        /// </summary>
        public DateTime ReviewStartDate { get; set; }

        /// <summary>
        /// UTC date
        /// </summary>
        public DateTime ReviewEndDate { get; set; }

        /// <summary>
        /// UTC dates
        /// </summary>
        public IEnumerable<DateTime> EmailTimings { get; set; } = new List<DateTime>();

        /// <summary>
        /// UTC dates
        /// </summary>
        public IList<DateTime> ProcessedTimings { get; set; } = new List<DateTime>();
    }
}
