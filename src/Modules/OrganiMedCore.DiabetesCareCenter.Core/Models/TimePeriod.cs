using IntelliMed.Core.Validators;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class TimePeriod
    {
        [Required]
        [JsonProperty("timeFrom")]
        public TimeSpan TimeFrom { get; set; }

        [Required]
        [GreaterThan(nameof(TimeFrom), ErrorMessage = "A rendelési idő vége nagyobb kell legyen, mint a kezdete.")]
        [JsonProperty("timeTo")]
        public TimeSpan TimeTo { get; set; }
    }
}
