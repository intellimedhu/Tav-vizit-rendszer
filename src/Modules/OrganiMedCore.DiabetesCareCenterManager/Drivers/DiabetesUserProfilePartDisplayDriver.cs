using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;

namespace OrganiMedCore.DiabetesCareCenterManager.Drivers
{
    public class DiabetesUserProfilePartDisplayDriver : ContentPartDisplayDriver<DiabetesUserProfilePart>
    {
        public override IDisplayResult Display(DiabetesUserProfilePart part)
        {
            var viewModel = new DiabetesUserProfilePartAdminViewModel();
            viewModel.UpdateViewModel(part);

            return Shape("DiabetesUserProfile_SummaryAdmin", viewModel).Location("SummaryAdmin", "Meta:10");
        }
    }
}
