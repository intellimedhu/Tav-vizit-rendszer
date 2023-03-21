using IntelliMed.Core.Extensions;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileEquipmentsSettingsViewModel
    {
        [JsonProperty("tools")]
        public IEnumerable<CenterProfileEquipmentViewModel> Tools { get; set; } = new List<CenterProfileEquipmentViewModel>();

        [JsonProperty("laboratory")]
        public IEnumerable<CenterProfileEquipmentViewModel> Laboratory { get; set; } = new List<CenterProfileEquipmentViewModel>();


        public void UpdateViewModel(CenterProfileEquipmentsSettings settings)
        {
            settings.ThrowIfNull();

            Tools = UpdateViewModelEquipments(settings.ToolsList);
            Laboratory = UpdateViewModelEquipments(settings.LaboratoryList);
        }

        public void UpdateModel(CenterProfileEquipmentsSettings settings)
        {
            settings.ThrowIfNull();

            settings.ToolsList = UpdateModelEquipments(Tools);
            settings.LaboratoryList = UpdateModelEquipments(Laboratory);
        }


        private IEnumerable<CenterProfileEquipmentViewModel> UpdateViewModelEquipments(IEnumerable<CenterProfileEquipmentSetting> equipments)
            => equipments.Select(equipment =>
            {
                var viewModel = new CenterProfileEquipmentViewModel();
                viewModel.UpdateViewModel(equipment);

                return viewModel;
            });

        private IEnumerable<CenterProfileEquipmentSetting> UpdateModelEquipments(IEnumerable<CenterProfileEquipmentViewModel> viewModels)
            => viewModels.Select(viewModel =>
            {
                var tool = new CenterProfileEquipmentSetting();
                viewModel.UpdateModel(tool);

                return tool;
            });
    }
}
