using Newtonsoft.Json;
using System.Collections.Generic;

namespace OrganiMedCore.Email.ViewModels
{
    public class EmailTemplateViewModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tokens")]
        public HashSet<string> Tokens { get; set; } = new HashSet<string>();
    }
}
