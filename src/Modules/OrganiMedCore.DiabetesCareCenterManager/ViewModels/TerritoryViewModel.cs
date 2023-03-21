using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class TerritoryViewModel
    {
        [JsonProperty("settlements")]
        public IEnumerable<SettlementDisplayViewModel> Settlements { get; set; } = new List<SettlementDisplayViewModel>();

        [JsonProperty("settlementsPool")]
        public IEnumerable<SettlementDisplayViewModel> SettlementsPool { get; set; } = new List<SettlementDisplayViewModel>();
    }
}
