using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenterManager.ViewModels
{
    public class PersonalConditionSettingsViewModel
    {
        public IEnumerable<PersonalConditionViewModel> Accredited { get; set; } = new List<PersonalConditionViewModel>();

        public IEnumerable<PersonalConditionViewModel> TemporarilyAccredited { get; set; } = new List<PersonalConditionViewModel>();


        public void UpdateViewModel(AccreditationPersonalConditionSettings model)
        {
            model.ThrowIfNull();

            Accredited = UpdateConditionViewModels(model.Accredited);
            TemporarilyAccredited = UpdateConditionViewModels(model.TemporarilyAccredited);
        }

        public void UpdateModel(AccreditationPersonalConditionSettings model)
        {
            model.ThrowIfNull();

            model.Accredited = UpdateConditionModels(Accredited);
            model.TemporarilyAccredited = UpdateConditionModels(TemporarilyAccredited);
        }


        private IEnumerable<PersonalConditionViewModel> UpdateConditionViewModels(IEnumerable<PersonalCondition> models)
            => models.Select(x =>
            {
                var viewModel = new PersonalConditionViewModel();
                viewModel.UpdateViewModel(x);

                return viewModel;
            });

        private IEnumerable<PersonalCondition> UpdateConditionModels(IEnumerable<PersonalConditionViewModel> viewModels)
            => viewModels
            .Where(x =>
                x.HeadCount.HasValue &&
                x.Occupation.HasValue &&
                x.HeadCount > 0)
            .Select(x => new PersonalCondition()
            {
                HeadCount = x.HeadCount.Value,
                Occupation = x.Occupation.Value
            });
    }
}
