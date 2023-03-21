using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.Services;
using OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.Organization.DiabetesCareCenter.Widgets")]
    public class CenterProfileOverviewInfoPartDisplayDriver : ContentPartDisplayDriver<CenterProfileOverviewInfoPart>
    {
        private readonly ICenterProfileInfoService _centerProfileEditorInfoService;


        public CenterProfileOverviewInfoPartDisplayDriver(ICenterProfileInfoService centerProfileEditorInfoService)
        {
            _centerProfileEditorInfoService = centerProfileEditorInfoService;
        }


        public override async Task<IDisplayResult> DisplayAsync(CenterProfileOverviewInfoPart part, BuildPartDisplayContext context)
        {
            // Getting the only "CenterProfileOverviewContainerBlock" and passing to the view for display.
            var (contentItem, isNew) = await _centerProfileEditorInfoService.GetOrCreateNewContentItemAsync(ContentTypes.CenterProfileOverviewContainerBlock);

            return Initialize<CenterProfileOverviewInfoPartViewModel>("CenterProfileOverviewInfo", viewModel =>
            {
                viewModel.ContentItem = contentItem;
            })
            .Location("Detail", "Content:5");
        }
    }
}
