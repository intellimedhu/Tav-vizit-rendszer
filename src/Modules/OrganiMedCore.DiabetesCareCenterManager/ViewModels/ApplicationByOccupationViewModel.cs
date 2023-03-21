using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class ApplicationByOccupationViewModel
    {
        public string CenterProfileContentItemId { get; set; }

        public Occupation? Occupation { get; set; }
    }
}
