using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class CenterProfileMakeDecisionViewModel
    {
        [JsonProperty("states")]
        public IEnumerable<CenterProfileDecisionStateViewModel> States { get; set; } = new List<CenterProfileDecisionStateViewModel>();
    }
}
