using Newtonsoft.Json;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class TerritoryEditViewModel
    {
        [JsonProperty("zipCode")]
        public int ZipCode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
