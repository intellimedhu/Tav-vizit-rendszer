using OrganiMedCore.DiabetesCareCenterManager.Constants;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class TerritoryCreateViewModel
    {
        [Required]
        public int? UserId { get; set; }

        [MaxLength(DataFieldsProperties.TerritoryNameMaxLength)]
        public string TerritoryName { get; set; }
    }
}
