using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Users;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Exceptions;
using OrganiMedCore.Login.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    public class HomeController : Controller, IUpdateModel
    {
        private readonly ICenterProfileCommonService _centerProfileCommonService;
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;
        private readonly INotifier _notifier;
        private readonly ISharedUserService _sharedUserService;
        private readonly SignInManager<IUser> _signInManager;


        public IHtmlLocalizer T { get; set; }


        public HomeController(
            ICenterProfileCommonService centerProfileCommonService,
            IDokiNetService dokiNetService,
            IHtmlLocalizer<HomeController> htmlLocalizer,
            ILogger<HomeController> logger,
            INotifier notifier,
            ISharedUserService sharedUserService,
            SignInManager<IUser> signInManager)
        {
            _centerProfileCommonService = centerProfileCommonService;
            _dokiNetService = dokiNetService;
            _logger = logger;
            _notifier = notifier;
            _sharedUserService = sharedUserService;
            _signInManager = signInManager;

            T = htmlLocalizer;
        }


        public IActionResult Index() => View();

        [Route("terkep")]
        public async Task<IActionResult> Map()
        {
            ViewData["GoogleMapsApiKey"] = (await _centerProfileCommonService.GetCenterManagerSettingsAsync()).GoogleMapsApiKey;

            return View();
        }

        public async Task<IActionResult> SessionLogin([FromQuery(Name = "web_id")]string nonce)
        {
            if (string.IsNullOrEmpty(nonce))
            {
                return RedirectToLogin();
            }

            try
            {
                var dokiNetMember = await _dokiNetService.GetDokiNetMemberByNonce<DokiNetMember>(nonce);
                if (dokiNetMember == null)
                {
                    return RedirectToLogin();
                }

                if (User.Identity.IsAuthenticated)
                {
                    await _signInManager.SignOutAsync();
                }

                if (!await _sharedUserService.SignInDokiNetMemberAsync(
                    dokiNetMember,
                    localizedMessage => _notifier.Error(T[localizedMessage]),
                    this,
                    false))
                {
                    return RedirectToLogin(false);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DokiNetMemberRegistrationException ex)
            {
                _notifier.Error(T[ex.Message]);

                return RedirectToLogin(false);
            }
            catch (HttpRequestException ex)
            {
                _notifier.Error(T["Sajnáljuk, a társasági rendszerrel történő kapcsolat hibára futott."]);
                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberByNonce", nonce);

                return RedirectToLogin(false);
            }
        }


        private IActionResult RedirectToLogin(bool addMessage = true)
        {
            if (addMessage)
            {
                _notifier.Error(T["A bejelentkezés nem sikerült."]);
            }

            return LocalRedirect("~/" + NamedRoutes.DokiNetLoginPath);
        }
    }
}
