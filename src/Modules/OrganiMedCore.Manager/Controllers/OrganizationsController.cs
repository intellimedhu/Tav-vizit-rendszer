using IntelliMed.Core.Constants;
using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganiMedCore.Manager.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Manager.Controllers
{
    public class OrganizationsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public OrganizationsController(
            IAuthorizationService authorizationService,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _authorizationService = authorizationService;
            _sharedDataAccessorService = sharedDataAccessorService;
        }


        //[Route("intezmenyek")]
        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Admin.Permissions.AccessAdminPanel))
            {
                return View("IndexUnauthorized");
            }

            var shellContexts = await _sharedDataAccessorService.ListShellContextsAsync();

            return View(shellContexts
                .Where(context => context.Settings.Name != WellKnownNames.ManagerTenantName)
                .Select(context => new OrganizationLinkViewModel()
                {
                    Name = context.Settings.Name,
                    Hostname = context.Settings.RequestUrlHost,
                    UrlPrefix = context.Settings.RequestUrlPrefix,
                    IsActivated = context.IsActivated
                }));
        }
    }
}
