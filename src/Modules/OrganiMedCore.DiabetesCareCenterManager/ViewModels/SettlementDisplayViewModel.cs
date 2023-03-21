using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class SettlementDisplayViewModel
    {
        [JsonProperty("zipCode")]
        public int ZipCode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("info")]
        public IEnumerable<string> Info { get; set; } = new List<string>();


        public static IEnumerable<SettlementDisplayViewModel> ToViewModel(IEnumerable<Settlement> settlements)
            => settlements
                .GroupBy(x => new { x.Name, x.ZipCode })
                .Select(x => new SettlementDisplayViewModel()
                {
                    Name = x.Key.Name,
                    ZipCode = x.Key.ZipCode,
                    Info = x.Select(y => y.Description).Where(y => !string.IsNullOrEmpty(y))
                });
    }
}
