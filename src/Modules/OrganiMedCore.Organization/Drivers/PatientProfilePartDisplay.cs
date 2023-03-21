using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.Drivers
{
    public class PatientProfilePartDisplay : ContentPartDisplayDriver<PatientProfilePart>
    {
        public override IDisplayResult Display(PatientProfilePart part)
        {
            return Initialize<PatientProfilePartViewModel>("PatientProfilePart", m => m.UpdateViewModel(part))
                .Location("Detail", "Content:1")
                .Location("Summary", "Content:1");
        }

        public override IDisplayResult Edit(PatientProfilePart part)
        {
            return Initialize<PatientProfilePartViewModel>("PatientProfilePart_Edit", m => m.UpdateViewModel(part));
        }

        public override async Task<IDisplayResult> UpdateAsync(PatientProfilePart part, IUpdateModel updater)
        {
            var viewModel = new PatientProfilePartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            viewModel.UpdatePart(part);

            return Edit(part);
        }
    }
}
