using IntelliMed.Core.Validators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class DailyOfficeHours
    {
        [Required]
        [JsonProperty("day")]
        public DayOfWeek Day { get; set; }

        [NotEmpty(ErrorMessage = "A rendelési idő kezdete és vége idők megadása kötelező")]
        [JsonProperty("hours")]
        public IEnumerable<TimePeriod> Hours { get; set; }
    }
}
