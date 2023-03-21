using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileColleagueActionViewModel
    {
        [Required]
        [JsonProperty("action")]
        public ColleagueAction ColleagueAction { get; set; }

        [Required]
        [JsonProperty("id")]
        public string ColleagueId { get; set; }
    }
}
