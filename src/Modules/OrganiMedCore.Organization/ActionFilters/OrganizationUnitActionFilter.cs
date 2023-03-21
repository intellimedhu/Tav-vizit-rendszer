using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.DisplayManagement.Notify;
using OrganiMedCore.Organization.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.ActionFilters
{
    /// <summary>
    /// Usage: [ServiceFilter(typeof(OrganizationUnitActionFilter))]
    /// </summary>
    public class OrganizationUnitActionFilter : IAsyncActionFilter
    {
        private readonly IOrganizationService _organizationService;
        private readonly IBetterUserService _betterUserService;
        private readonly INotifier _notifier;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public IHtmlLocalizer T { get; set; }


        public OrganizationUnitActionFilter(
            IOrganizationService organizationService,
            IBetterUserService betterUserService,
            INotifier notifier,
            IHtmlLocalizer<OrganizationUnitActionFilter> htmlLocalizer,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _organizationService = organizationService;
            _betterUserService = betterUserService;
            _notifier = notifier;
            _sharedDataAccessorService = sharedDataAccessorService;

            T = htmlLocalizer;
        }


        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                if (await _organizationService.GetSignedInOrganizationUnitAsync(scope, await _betterUserService.GetCurrentUserAsync()) == null)
                {
                    _notifier.Error(T["Először be kell jelentkezni egy osztályba."]);
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "OrganizationUnitSwitcher",
                        action = "Index",
                        area = "OrganiMedCore.Organization"
                    }));
                }
                else
                {
                    var resultContext = await next();
                }
            }
        }
    }
}