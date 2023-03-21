using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Users.ViewModels;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Services
{
    // TODO: add unit tests
    public class DokiNetLoginService : IDokiNetLoginService
    {
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger<DokiNetLoginService> _logger;
        private readonly INotifier _notifier;
        private readonly ISharedUserService _sharedUserService;


        public IHtmlLocalizer T { get; set; }


        public DokiNetLoginService(
            IDokiNetService dokiNetService,
            IHtmlLocalizer<DokiNetLoginService> htmlLocalizer,
            ILogger<DokiNetLoginService> logger,
            ISharedUserService sharedUserService)
        {
            _dokiNetService = dokiNetService;
            _logger = logger;
            _sharedUserService = sharedUserService;

            T = htmlLocalizer;
        }


        public async Task<bool> TryDokiNetLoginAsync(LoginViewModel viewModel, IUpdateModel updater)
        {
            DokiNetMember dokiNetMember;
            try
            {
                dokiNetMember = await _dokiNetService.GetDokiNetMemberByLoginAsync<DokiNetMember>(viewModel.UserName, viewModel.Password);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberByLoginAsync", viewModel.UserName);

                updater.ModelState.AddModelError(string.Empty, T["Hiba történt a társasági rendszerrel történő kapcsolat során."].Value);

                return false;
            }

            if (dokiNetMember == null)
            {
                updater.ModelState.AddModelError(string.Empty, T["Sikertelen bejelentkezés."].Value);

                return false;
            }

            return await _sharedUserService.SignInDokiNetMemberAsync(
                dokiNetMember,
                localizedMessage => _notifier.Error(T[localizedMessage]),
                updater,
                viewModel.RememberMe);
        }
    }
}
