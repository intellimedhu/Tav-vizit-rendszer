using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class ColleagueStatusItem
    {
        [JsonProperty("status")]
        public ColleagueStatus Status { get; set; }

        [JsonProperty("statusDateUtc")]
        public DateTime? StatusDateUtc { get; set; }
    }
}
