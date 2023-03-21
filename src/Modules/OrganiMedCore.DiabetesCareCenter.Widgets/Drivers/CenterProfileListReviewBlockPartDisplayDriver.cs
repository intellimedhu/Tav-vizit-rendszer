using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Drivers
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class CenterProfileListReviewBlockPartDisplayDriver : ContentPartDisplayDriver<CenterProfileListReviewBlockPart>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IBetterUserService _betterUserService;
        private readonly ICenterProfileReviewService _centerProfileReviewService;
        private readonly ICenterProfileService _centerProfileService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CenterProfileListReviewBlockPartDisplayDriver(
            IAuthorizationService authorizationService,
            IBetterUserService betterUserService,
            ICenterProfileReviewService centerProfileReviewService,
            ICenterProfileService centerProfileService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _betterUserService = betterUserService;
            _centerProfileReviewService = centerProfileReviewService;
            _centerProfileService = centerProfileService;
            _httpContextAccessor = httpContextAccessor;
        }


        public override async Task<IDisplayResult> DisplayAsync(CenterProfileListReviewBlockPart part, BuildPartDisplayContext context)
        {
            var contentItems = Enumerable.Empty<ContentItem>();
            if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, ManagerPermissions.ViewAllCenterProfiles))
            {
                contentItems = await _centerProfileService.GetCenterProfilesAsync();
            }
            else if (await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, ManagerPermissions.ViewListOfCenterProfiles))
            {
                contentItems = await _centerProfileService.GetPermittedCenterProfilesAsync();
            }

            var reviewerStatistics = await _centerProfileReviewService.GetReviewerStatisticsAsync(
                await _betterUserService.GetCurrentUserAsync(),
                contentItems);

            return Initialize<CenterProfileListReviewStatisticsViewModel>("CenterProfileListReviewBlock", viewModel =>
            {
                viewModel.ReviewerStatistics = reviewerStatistics;
            })
            .Location("Content:5");
        }
    }
}
