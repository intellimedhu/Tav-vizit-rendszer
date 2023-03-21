using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Core.Drivers
{
    public class CenterProfilePartDisplayDriver : ContentPartDisplayDriver<CenterProfilePart>
    {
        private readonly IDokiNetService _dokiNetService;


        public CenterProfilePartDisplayDriver(IDokiNetService dokiNetService)
        {
            _dokiNetService = dokiNetService;
        }


        public override async Task<IDisplayResult> DisplayAsync(CenterProfilePart part, BuildPartDisplayContext context)
        {
            var member = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(part.MemberRightId);
            var viewModelComplex = CenterProfileComplexViewModel.CreateViewModel(part.ContentItem, true, true, true, true, true, member);
            var viewModelSummaryAdmin = new CenterProfilePartViewModel();
            viewModelSummaryAdmin.UpdateViewModel(part);
            viewModelSummaryAdmin.LeaderName = member.FullName;


            return Combine(
                Shape("CenterProfile", viewModelComplex).Location("Detail", "Content:1"),
                Shape("CenterProfile_Summary", viewModelComplex).Location("Summary", "Content:1"),
                Shape("CenterProfile_SummaryAdmin", viewModelSummaryAdmin).Location("SummaryAdmin", "Meta:1")
            );
        }
    }
}
