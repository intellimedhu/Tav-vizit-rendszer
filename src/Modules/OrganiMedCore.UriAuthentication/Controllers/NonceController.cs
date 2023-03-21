using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Users;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Services;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.UriAuthentication.Controllers
{
    [Route("nc", Name = "OrganiMedCore.UriAuthentication")]
    public class NonceController : Controller, IUpdateModel
    {
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;
        private readonly INonceService _nonceService;
        private readonly INotifier _notifier;
        private readonly ISharedUserService _sharedUserService;
        private readonly SignInManager<IUser> _signInManager;

        private const string ViewInvalidNonce = "InvalidNonce";
        private const string ViewError = "Error";


        public IHtmlLocalizer T { get; set; }


        public NonceController(
            IDokiNetService dokiNetService,
            IHtmlLocalizer<NonceController> htmlLocalizer,
            ILogger<NonceController> logger,
            INonceService nonceService,
            INotifier notifier,
            ISharedUserService sharedUserService,
            SignInManager<IUser> signInManager)
        {
            _dokiNetService = dokiNetService;
            _logger = logger;
            _nonceService = nonceService;
            _notifier = notifier;
            _sharedUserService = sharedUserService;
            _signInManager = signInManager;

            T = htmlLocalizer;
        }


        [Route("{value}")]
        public async Task<IActionResult> Index(string value)
        {
            if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var nonceValue))
            {
                return View(ViewInvalidNonce);
            }

            var nonce = await _nonceService.GetByValue(nonceValue);
            if (nonce == null)
            {
                return View(ViewInvalidNonce);
            }

            if (nonce.Type == NonceType.MemberRightId)
            {
                try
                {
                    return await SignInDokiNetMemberAsync(nonce);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "IDokiNetService.GetDokiNetMemberById", "MRID:" + nonce.TypeId, "Nonce:" + value);

                    ViewData["ErrorMessage"] = "Hiba történt a társasági rendszerrel történő kapcsolat során.";

                    return View(ViewError);
                }
            }

            // TODO: bővíteni (factory?), ha szükség lesz több féle típusra:
            //if(nonce.Type == NonceType.MemberId)
            //if (nonce.Type == NonceType.UserId)
            //{
            //    _userManager.FindByIdAsync()
            //}

            return View(ViewInvalidNonce);
        }


        private async Task<IActionResult> SignInDokiNetMemberAsync(Nonce nonce)
        {
            var dokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(nonce.TypeId);
            if (dokiNetMember == null)
            {
                _logger.LogError("DokiNetMember not found by memberRightId", nonce.TypeId);
                ViewData["ErrorMessage"] = T["A felhasználó nem azonosítható."].Value;

                return View(ViewError);
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
                // TODO: ez így nem jó. Unathorized() fog kelleni miután ez az issue megvan:
                // https://github.com/OrchardCMS/OrchardCore/issues/3209
                return LocalRedirect("~/" + NamedRoutes.DokiNetLoginPath);
            }

            return LocalRedirect(nonce.RedirectUrl);
        }
    }
}
