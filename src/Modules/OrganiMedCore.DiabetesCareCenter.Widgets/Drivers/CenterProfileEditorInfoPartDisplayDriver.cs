using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Widgets.Services;
using OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    public abstract class CenterProfileEditorInfoPartDisplayDriver : ContentPartDisplayDriver<CenterProfileEditorInfoPart>
    {
        protected readonly ICenterProfileInfoService _centerProfileEditorInfoService;


        protected abstract DiabetesCareCenterTenants DiabetesCareCenterTenants { get; }


        public CenterProfileEditorInfoPartDisplayDriver(ICenterProfileInfoService centerProfileEditorInfoService)
        {
            _centerProfileEditorInfoService = centerProfileEditorInfoService;
        }


        public override async Task<IDisplayResult> DisplayAsync(CenterProfileEditorInfoPart part, BuildPartDisplayContext context)
        {
            // Getting the only "CenterProfileEditorInfoBlockContainer" and passing to the view for display.
            var (contentItem, isNew) = await _centerProfileEditorInfoService.GetOrCreateNewContentItemAsync(ContentTypes.CenterProfileEditorInfoBlockContainer);

            return Initialize<CenterProfileEditorInfoPartViewModel>("CenterProfileEditorInfo", viewModel =>
            {
                viewModel.ContentItem = contentItem;
                viewModel.DiabetesCareCenterTenants = DiabetesCareCenterTenants;
            })
            .Location("Detail", "Content:5");
        }
    }
}
