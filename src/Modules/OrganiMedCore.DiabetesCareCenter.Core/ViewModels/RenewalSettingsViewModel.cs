using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class RenewalSettingsViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public DateTime? RenewalStartDate { get; set; }

        [Required]
        public DateTime? ReviewStartDate { get; set; }

        [Required]
        public DateTime? ReviewEndDate { get; set; }

        public IEnumerable<DateTime> EmailTimings { get; set; } = new List<DateTime>();

        public IEnumerable<DateTime> ProcessedTimings { get; set; } = new List<DateTime>();


        public void UpdateViewModel(RenewalPeriod renewalPeriod)
        {
            renewalPeriod.ThrowIfNull();

            Id = renewalPeriod.Id;
            RenewalStartDate = renewalPeriod.RenewalStartDate;
            ReviewStartDate = renewalPeriod.ReviewStartDate;
            ReviewEndDate = renewalPeriod.ReviewEndDate;
            EmailTimings = renewalPeriod.EmailTimings;
            ProcessedTimings = renewalPeriod.ProcessedTimings;
        }

        public void UpdateModel(RenewalPeriod renewalPeriod)
        {
            renewalPeriod.ThrowIfNull();
            if (RenewalStartDate.HasValue)
            {
                renewalPeriod.RenewalStartDate = DateTime.SpecifyKind(RenewalStartDate.Value.ToUniversalTime(), DateTimeKind.Utc);
            }

            if (ReviewStartDate.HasValue)
            {
                renewalPeriod.ReviewStartDate = DateTime.SpecifyKind(ReviewStartDate.Value.ToUniversalTime(), DateTimeKind.Utc);
            }

            if (ReviewEndDate.HasValue)
            {
                renewalPeriod.ReviewEndDate = DateTime.SpecifyKind(ReviewEndDate.Value.ToUniversalTime(), DateTimeKind.Utc);
            }

            renewalPeriod.EmailTimings = EmailTimings
                .Select(x => DateTime.SpecifyKind(x.ToUniversalTime(), DateTimeKind.Utc))
                .OrderBy(x => x);
        }
    }
}
