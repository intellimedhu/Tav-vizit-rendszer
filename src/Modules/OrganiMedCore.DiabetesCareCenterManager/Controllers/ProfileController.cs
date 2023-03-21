using IntelliMed.DokiNetIntegration.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrganiMedCore.DiabetesCareCenterManager.Extensions;
using OrganiMedCore.Login.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Authorize]
    public class ProfileController : Controller, IUpdateModel
    {
        private readonly ILogger _logger;
        private readonly INotifier _notifier;
        private readonly ISharedUserService _sharedUserService;


        public IHtmlLocalizer T { get; set; }


        public ProfileController(
            IHtmlLocalizer<ProfileController> htmlLocalizer,
            ILogger<ProfileController> logger,
            INotifier notifier,
            ISharedUserService sharedUserService)
        {
            _logger = logger;
            _notifier = notifier;
            _sharedUserService = sharedUserService;

            T = htmlLocalizer;
        }


        [Route("szeh-adataim")]
        public async Task<IActionResult> Index()
        {
            try
            {
                await _sharedUserService.UpdateAndGetCurrentUsersDokiNetMemberDataAsync();

                return View();
            }
            catch (UserHasNoMemberRightIdException)
            {
                return this.RedirectWhenUserIsNotDokiNetMember(_notifier, T);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById");

                _notifier.Error(T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);

                return this.RedirectToHome();
            }
        }
    }
}
