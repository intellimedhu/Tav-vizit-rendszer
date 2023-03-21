using IntelliMed.Core.Services;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Tenants.ViewModels;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using OrganiMedCore.DiabetesCareCenterManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Authorize]
    [Admin]
    public class CenterProfileAdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        private readonly ICenterProfileService _centerProfileService;
        private readonly IClock _clock;
        private readonly IDokiNetService _dokiNetService;
        private readonly INotifier _notifier;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        private const string SzehTenantRecipe = "OrganiMedCore DiabetesCareCenter Tenant - Production";


        public IHtmlLocalizer T { get; set; }


        public CenterProfileAdminController(
            IAuthorizationService authorizationService,
            IContentManager contentManager,
            ICenterProfileService centerProfileService,
            IClock clock,
            IDokiNetService dokiNetService,
            IHtmlLocalizer<CenterProfileAdminController> stringLocalizer,
            INotifier notifier,
            ISharedDataAccessorService sharedDataAccessorService)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            _centerProfileService = centerProfileService;
            _clock = clock;
            _dokiNetService = dokiNetService;
            _notifier = notifier;
            _sharedDataAccessorService = sharedDataAccessorService;

            T = stringLocalizer;
        }


        public async Task<IActionResult> List()
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfileTenant))
            {
                return Unauthorized();
            }

            var contentItems = await _centerProfileService.GetCenterProfilesAsync();

            IEnumerable<ShellSettingsEntry> entries = null;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var dataProtector = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>()
                    .CreateProtector("Tokens")
                    .ToTimeLimitedDataProtector();

                var allSettings = scope.ServiceProvider.GetRequiredService<IShellHost>().GetAllSettings();
                entries = allSettings
                    .Where(shellSettings => shellSettings["RecipeName"] == SzehTenantRecipe)
                    .Select(shellSettings =>
                    {
                        var entry = new ShellSettingsEntry()
                        {
                            Name = shellSettings.Name,
                            ShellSettings = shellSettings
                        };

                        if (shellSettings.State == TenantState.Uninitialized && !string.IsNullOrEmpty(shellSettings["Secret"]))
                        {
                            entry.Token = dataProtector.Protect(shellSettings["Secret"], _clock.UtcNow.Add(new TimeSpan(24, 0, 0)));
                        }

                        return entry;
                    });
            }

            ViewData["Entries"] = entries;

            return View(contentItems.Select(contentItem =>
                CenterProfileComplexViewModel.CreateViewModel(contentItem, basicData: true, renewal: true)));
        }

        public async Task<IActionResult> CreateTenant(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfileTenant))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var contentItem = await _centerProfileService.GetCenterProfileAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }

            ViewData["Complex"] = CenterProfileComplexViewModel.CreateViewModel(contentItem, basicData: true, renewal: true);

            return View();
        }

        [HttpPost, ActionName(nameof(CreateTenant))]
        public async Task<IActionResult> CreateTenantPost(string id, EditTenantViewModel viewModel)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.CreateCenterProfileTenant))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var contentItem = await _centerProfileService.GetCenterProfileAsync(id);
            if (contentItem == null)
            {
                return NotFound();
            }

            var managerPart = contentItem.As<CenterProfileManagerExtensionsPart>();
            if (!string.IsNullOrEmpty(managerPart.AssignedTenantName))
            {
                _notifier.Error(T["A szeh tenant már hozzá lett rendelve az adatlaphoz."]);

                return RedirectToAction(nameof(List));
            }

            await ValidateViewModelAsync(viewModel);

            if (!ModelState.IsValid)
            {
                ViewData["Complex"] = CenterProfileComplexViewModel.CreateViewModel(contentItem, basicData: true, renewal: true);

                return View(viewModel);
            }

            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var shellSettingsManager = scope.ServiceProvider.GetRequiredService<IShellSettingsManager>();
                var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();

                var shellSettings = shellSettingsManager.CreateDefaultSettings();

                shellSettings.Name = viewModel.Name;
                shellSettings.RequestUrlHost = null;
                shellSettings.RequestUrlPrefix = viewModel.RequestUrlPrefix;
                shellSettings.State = TenantState.Uninitialized;

                shellSettings["ConnectionString"] = null;
                shellSettings["TablePrefix"] = null;
                shellSettings["DatabaseProvider"] = "Sqlite";
                shellSettings["Secret"] = Guid.NewGuid().ToString();

                // TODO: throw ex if not exists:
                shellSettings["RecipeName"] = SzehTenantRecipe;

                shellSettingsManager.SaveSettings(shellSettings);
                var shellContext = await shellHost.GetOrCreateShellContextAsync(shellSettings);
            }

            contentItem.Alter<CenterProfileManagerExtensionsPart>(part =>
            {
                part.AssignedTenantName = viewModel.Name;
            });

            await _contentManager.UpdateAsync(contentItem);
            _centerProfileService.ClearCenterProfileCache();

            _notifier.Success(T["A szeh tenant létrehozása sikeres volt."]);

            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> DownloadMembershipTable()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            var centerProfiles = await _centerProfileService.GetCenterProfilesAsync();
            var data = (await Task.WhenAll(centerProfiles.Select(async contentItem =>
            {
                var part = contentItem.As<CenterProfilePart>();

                var memberRightIds = new List<int>()
                {
                    part.MemberRightId
                };

                memberRightIds.AddRange(part.Colleagues.Where(x => x.MemberRightId.HasValue).Select(x => x.MemberRightId.Value));

                var members = await _dokiNetService.GetDokiNetMembersByIds<DokiNetMember>(memberRightIds);
                var statusTexts = ColleagueStatusExtensions.GetLocalizedValues(T);

                return members.Select(member =>
                {
                    var status = part.Colleagues.FirstOrDefault(x => x.MemberRightId == member.MemberRightId)?.LatestStatusItem.Status;
                    var statusText = string.Empty;
                    if (status.HasValue && statusTexts.ContainsKey(status.Value))
                    if (status.HasValue && statusTexts.ContainsKey(status.Value))
                    {
                        statusText = statusTexts[status.Value];
                    }

                    return new
                    {
                        part.CenterName,
                        member.FullName,
                        Email = member.Emails.FirstOrDefault(),
                        IsLeader = part.MemberRightId == member.MemberRightId ? T["Igen"].Value : "-",
                        HasMembership = member.HasMemberShip ? T["Igen"].Value : "-",
                        IsMembershipFeePaid = member.HasMemberShip
                                ? (member.IsDueOk ? T["Igen"].Value : "-")
                                : T["nem értelmezett"].Value,
                        Status = statusText
                    };
                });
            })))
            .SelectMany(x => x)
            .OrderBy(x => x.CenterName)
            .ThenByDescending(x => x.IsLeader)
            .ThenBy(x => x.FullName)
            .ToArray();

            var bytes = data.SimpleDataExport(T["Tagság, tagdíj"].Value);
            new FileExtensionContentTypeProvider().TryGetContentType("t.xlsx", out var contentType);

            return File(bytes, contentType, $"tagsag-tagdij-{_clock.UtcNow.ToString("yyyyMMdd")}.xlsx");
        }


        private async Task ValidateViewModelAsync(EditTenantViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.Name))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), T["The tenant name is mandatory."].Value);
            }

            IEnumerable<ShellSettings> allSettings = null;
            using (var scope = await _sharedDataAccessorService.GetManagerServiceScopeAsync())
            {
                var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();
                allSettings = shellHost.GetAllSettings();
            }

            if (allSettings.Any(tenant => string.Equals(tenant.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), T["A tenant with the same name already exists.", viewModel.Name].Value);
            }

            if (!string.IsNullOrEmpty(viewModel.Name) && !Regex.IsMatch(viewModel.Name, @"^[a-zA-Z\d]+$"))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.Name), T["Invalid tenant name. Must contain  characters only and no spaces."].Value);
            }

            var hasPrefix = !string.IsNullOrWhiteSpace(viewModel.RequestUrlPrefix);
            if (hasPrefix && !Regex.IsMatch(viewModel.RequestUrlPrefix, @"^[a-z\-\d]+$"))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), T["Invalid prefix. Must contain lower case characters, dash, numbers only and no spaces."].Value);
            }

            var allOtherShells = allSettings.Where(tenant => !string.Equals(tenant.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase));
            if (allOtherShells.Any(tenant => string.Equals(tenant.RequestUrlPrefix, viewModel.RequestUrlPrefix?.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), T["A tenant with the same prefix already exists.", viewModel.Name].Value);
            }

            if (hasPrefix)
            {
                if (viewModel.RequestUrlPrefix.Contains('/'))
                {
                    ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), T["The url prefix can not contain more than one segment."].Value);
                }
            }
        }
    }
}
