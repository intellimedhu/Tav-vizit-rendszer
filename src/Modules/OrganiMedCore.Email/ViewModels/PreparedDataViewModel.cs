using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrganiMedCore.Email.ViewModels
{
    public class PreparedDataViewModel
    {
        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [JsonProperty("preparedData")]
        public string PreparedData { get; set; }
    }
}
