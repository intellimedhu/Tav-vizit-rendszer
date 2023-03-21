using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileColleaguesViewModel : ICenterProfileViewModel
    {
        public IEnumerable<CenterProfileColleagueViewModel> Colleagues { get; set; } = new List<CenterProfileColleagueViewModel>();


        public void UpdatePart(CenterProfilePart part)
        {
        }

        public void UpdateViewModel(CenterProfilePart part)
        {
            part.ThrowIfNull();

            Colleagues = part.Colleagues.Select(model =>
            {
                var viewModel = new CenterProfileColleagueViewModel();
                viewModel.UpdateViewModel(model);

                return viewModel;
            });
        }
    }
}
