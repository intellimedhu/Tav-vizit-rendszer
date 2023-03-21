using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenter.Core.Models
{
    public class CenterProfileEquipmentSetting
    {
        public string Id { get; set; }

        [Required]
        public string Caption { get; set; }

        public bool Required { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Order { get; set; }

        [Required]
        public EquipmentType Type { get; set; }
    }
}
