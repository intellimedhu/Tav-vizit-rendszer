using IntelliMed.Core.Extensions;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileEquipmentsViewModel : ICenterProfileViewModel
    {
        [JsonProperty("tools")]
        public IEnumerable<CenterProfileEquipment<int?>> Tools { get; set; } = new List<CenterProfileEquipment<int?>>();

        [JsonProperty("laboratory")]
        public IEnumerable<CenterProfileEquipment<bool>> Laboratory { get; set; } = new List<CenterProfileEquipment<bool>>();

        [JsonProperty("backgroundConcilium")]
        public bool BackgroundConcilium { get; set; }

        [JsonProperty("backgroundInpatient")]
        public bool BackgroundInpatient { get; set; }


        public void UpdatePart(CenterProfilePart part)
        {
            part.ThrowIfNull();

            part.Tools = Tools;
            part.Laboratory = Laboratory;
            part.BackgroundConcilium = BackgroundConcilium;
            part.BackgroundInpatient = BackgroundInpatient;
        }

        public void UpdateViewModel(CenterProfilePart part)
        {
            part.ThrowIfNull();

            Tools = part.Tools;
            Laboratory = part.Laboratory;
            BackgroundConcilium = part.BackgroundConcilium;
            BackgroundInpatient = part.BackgroundInpatient;
        }
    }
}
