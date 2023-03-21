using Newtonsoft.Json;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class CenterProfileEditorSearchViewModel
    {
        [JsonProperty("zipCode")]
        public int ZipCode { get; set; }

        [JsonProperty("settlement")]
        public string Settlement { get; set; }

        [JsonProperty("territorialRapporteur")]
        public string TerritorialRapporteur { get; set; }
    }
}
