using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileDecisionStateViewModel
    {
        [JsonProperty("accreditationStatus")]
        public AccreditationStatus? AccreditationStatus { get; set; }

        [Required]
        [JsonProperty("contentItemId")]
        public string ContentItemId { get; set; }
    }
}
