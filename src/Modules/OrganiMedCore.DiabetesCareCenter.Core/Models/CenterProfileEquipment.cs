using Newtonsoft.Json;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class CenterProfileEquipment<T>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("value")]
        public T Value { get; set; }
    }
}
