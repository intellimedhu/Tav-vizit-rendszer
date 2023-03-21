using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Collections.Generic;

namespace OrganiMedCore.DiabetesCareCenter.Core.Settings
{
    public class CenterProfileEquipmentsSettings
    {
        public IEnumerable<CenterProfileEquipmentSetting> ToolsList { get; set; } = new List<CenterProfileEquipmentSetting>();

        public IEnumerable<CenterProfileEquipmentSetting> LaboratoryList { get; set; } = new List<CenterProfileEquipmentSetting>();
    }
}
