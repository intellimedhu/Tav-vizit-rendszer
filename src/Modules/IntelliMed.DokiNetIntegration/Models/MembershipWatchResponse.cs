using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IntelliMed.DokiNetIntegration.Models
{
    public class MembershipWatchResponse
    {
        [JsonProperty("MemberRightList")]
        public IEnumerable<int> MemberRightIds { get; set; } = new List<int>();

        /// <summary>
        /// Local datetime of the doki.Net server.
        /// </summary>
        [JsonProperty("ServerDate")]
        public DateTime LastCheckDate { get; set; }
    }
}
