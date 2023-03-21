using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class CenterProfileApplicationViewModel
    {
        [Required]
        public string ContentItemId { get; set; }

        [Required(ErrorMessage = "A munkakör kiválasztása kötelező.")]
        public Occupation? Occupation { get; set; }
    }
}
