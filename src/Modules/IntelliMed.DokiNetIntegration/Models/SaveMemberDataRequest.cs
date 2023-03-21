using Newtonsoft.Json;
using System.Collections.Generic;

namespace IntelliMed.DokiNetIntegration.Models
{
    public class SaveMemberDataRequest
    {
        [JsonProperty("Member_ID")]
        public int MemberId { get; set; }

        [JsonProperty("MemberDataList")]
        public IEnumerable<MemberData> Values { get; set; } = new List<MemberData>();
    }
}
