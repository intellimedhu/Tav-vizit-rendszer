using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class PersonalConditionViewModel
    {
        public Occupation? Occupation { get; set; }

        public int? HeadCount { get; set; }


        public void UpdateViewModel(PersonalCondition model)
        {
            model.ThrowIfNull();

            Occupation = model.Occupation;
            HeadCount = model.HeadCount;
        }
    }
}
