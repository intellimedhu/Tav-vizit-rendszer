using Fluid;
using Fluid.Values;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Http;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Filters
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class CenterProfileReviewFilter : ILiquidFilter
    {
        private readonly IBetterUserService _betterUserService;
        private readonly ICenterProfileReviewService _centerProfileReviewService;
        private readonly ICenterProfileService _centerProfileService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CenterProfileReviewFilter(
            IBetterUserService betterUserService,
            ICenterProfileReviewService centerProfileReviewService,
            ICenterProfileService centerProfileService,
            IHttpContextAccessor httpContextAccessor)
        {
            _betterUserService = betterUserService;
            _centerProfileReviewService = centerProfileReviewService;
            _centerProfileService = centerProfileService;
            _httpContextAccessor = httpContextAccessor;
        }


        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!(input.ToObjectValue() is ClaimsPrincipal principal) ||
                !(await _betterUserService.ConvertUserAsync(principal) is User user))
            {
                return BooleanValue.False;
            }

            // Accessing the currently displayed profile using its id from the URL.
            var contentItemId = _httpContextAccessor.HttpContext.Request.Query["id"];
            if (string.IsNullOrEmpty(contentItemId))
            {
                return BooleanValue.False;
            }

            var contentItem = await _centerProfileService.GetCenterProfileAsync(contentItemId);
            if (contentItem == null)
            {
                return BooleanValue.False;
            }

            var authorizationResult = await _centerProfileReviewService.GetAuthorizationResultAsync(user, contentItem);

            return authorizationResult.IsAuthorized
                ? BooleanValue.True
                : BooleanValue.False;
        }
    }
}
