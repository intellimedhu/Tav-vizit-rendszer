using Newtonsoft.Json;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileLeaderViewModel
    {
        [JsonProperty("memberRightId")]
        public int? MemberRightId { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
