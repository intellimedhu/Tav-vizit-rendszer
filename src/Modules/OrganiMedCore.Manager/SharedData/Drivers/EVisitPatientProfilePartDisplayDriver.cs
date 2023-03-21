using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.Manager.SharedData.Drivers
{
    public class EVisitPatientProfilePartDisplayDriver : ContentPartDisplayDriver<EVisitPatientProfilePart>
    {
        public override IDisplayResult Display(EVisitPatientProfilePart part)
        {
            return Initialize<EVisitPatientProfilePartViewModel>("EVisitPatientProfilePart", m => m.UpdateViewModel(part))
                .Location("Detail", "Content:1")
                .Location("Summary", "Content:1");
        }

        public override IDisplayResult Edit(EVisitPatientProfilePart part)
        {
            return Initialize<EVisitPatientProfilePartViewModel>("EVisitPatientProfilePart_Edit", m => m.UpdateViewModel(part));
        }

        public override async Task<IDisplayResult> UpdateAsync(EVisitPatientProfilePart part, IUpdateModel updater)
        {
            var viewModel = new EVisitPatientProfilePartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            viewModel.UpdatePart(part);

            return Edit(part);
        }
    }
}
