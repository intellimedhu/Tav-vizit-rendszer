using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class SettlementEditResponseViewModel
    {
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("settlements")]
        public IEnumerable<SettlementEditViewModel> Settlements { get; set; } = Enumerable.Empty<SettlementEditViewModel>();
    }
}
