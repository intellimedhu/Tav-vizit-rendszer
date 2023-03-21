using IntelliMed.Core.Extensions;
using Newtonsoft.Json;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileEquipmentViewModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("type")]
        public EquipmentType Type { get; set; }


        public void UpdateViewModel(CenterProfileEquipmentSetting tool)
        {
            tool.ThrowIfNull();

            Id = tool.Id;
            Caption = tool.Caption;
            Required = tool.Required;
            Order = tool.Order;
            Type = tool.Type;
        }

        public void UpdateModel(CenterProfileEquipmentSetting tool)
        {
            tool.ThrowIfNull();

            tool.Id = Id;
            tool.Caption = Caption;
            tool.Required = Required;
            tool.Order = Order;
            tool.Type = Type;
        }
    }
}
