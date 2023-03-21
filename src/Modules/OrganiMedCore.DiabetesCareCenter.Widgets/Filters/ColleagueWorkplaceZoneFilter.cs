using Fluid;
using Fluid.Values;
using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Widgets.Services;
using OrganiMedCore.Login.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets.Filters
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class ColleagueWorkplaceZoneFilter : ILiquidFilter
    {
        private readonly IBetterUserService _betterUserService;
        private readonly ICenterProfileService _centerProfileService;
        private readonly IColleagueWorkplaceZoneService _colleagueWorkplaceZoneService;
        private readonly ISharedUserService _sharedUserService;


        public ColleagueWorkplaceZoneFilter(
            IBetterUserService betterUserService,
            ICenterProfileService centerProfileService,
            IColleagueWorkplaceZoneService colleagueWorkplaceZoneService,
            ISharedUserService sharedUserService)
        {
            _betterUserService = betterUserService;
            _centerProfileService = centerProfileService;
            _colleagueWorkplaceZoneService = colleagueWorkplaceZoneService;
            _sharedUserService = sharedUserService;
        }


        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!(input.ToObjectValue() is ClaimsPrincipal principal) ||
                !(await _betterUserService.ConvertUserAsync(principal) is User user) ||
                !(await _sharedUserService.GetUsersDokiNetMemberAsync(user) is DokiNetMember dokiNetMember))
            {
                return new ObjectValue(_colleagueWorkplaceZoneService.Default);
            }

            var contentItems = (await _centerProfileService.GetCenterProfilesAsync())
                .Where(contentItem => contentItem.As<CenterProfilePart>()
                    .Colleagues.Any(colleague => colleague.MemberRightId == dokiNetMember.MemberRightId))
                .ToArray();

            return new ObjectValue(!contentItems.Any()
                    ? _colleagueWorkplaceZoneService.Default
                    : _colleagueWorkplaceZoneService.GetZones(contentItems, dokiNetMember));
        }
    }
}
