using Newtonsoft.Json;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileEditorTerritorySearchViewModel
    {
        [JsonProperty("zipCode")]
        public int ZipCode { get; set; }

        [JsonProperty("settlement")]
        public string Settlement { get; set; }

        [JsonProperty("territorialRapporteur")]
        public string TerritorialRapporteur { get; set; }
    }
}
