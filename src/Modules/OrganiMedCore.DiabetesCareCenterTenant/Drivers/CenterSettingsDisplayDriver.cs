using IntelliMed.Core.Exceptions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterTenant.Settings;
using OrganiMedCore.DiabetesCareCenterTenant.ViewModels;
using OrganiMedCore.Login.Exceptions;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterTenant.Drivers
{
    public class CenterSettingsDisplayDriver : SectionDisplayDriver<ISite, CenterSettings>
    {
        private readonly ICenterProfileManagerService _centerManagerService;
        private readonly IDiabetesCareCenterService _diabetesOrganizationService;
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;


        public IStringLocalizer T { get; set; }


        public CenterSettingsDisplayDriver(
            ICenterProfileManagerService centerManagerService,
            IDiabetesCareCenterService diabetesOrganizationService,
            IDokiNetService dokiNetService,
            ILogger<CenterSettingsDisplayDriver> logger,
            IStringLocalizer<CenterSettingsDisplayDriver> stringLocalizer)
        {
            _centerManagerService = centerManagerService;
            _diabetesOrganizationService = diabetesOrganizationService;
            _dokiNetService = dokiNetService;
            _logger = logger;

            T = stringLocalizer;
        }


        public override async Task<IDisplayResult> EditAsync(CenterSettings section, BuildEditorContext context)
        {
            var centerProfileParts = Enumerable.Empty<CenterProfilePart>();
            var tenantUnavailable = false;
            try
            {
                centerProfileParts = await _centerManagerService.GetCenterProfilesAsync();
            }
            catch (TenantUnavailableException)
            {
                tenantUnavailable = true;
            }

            return Initialize<CenterSettingsViewModel>("CenterSettings_Edit", viewModel =>
            {
                viewModel.CenterProfileContentItemId = section.CenterProfileContentItemId;
                viewModel.CenterProfiles = centerProfileParts;
                viewModel.TenantUnavailable = tenantUnavailable;
            })
            .Location("Content:2")
            .OnGroup(Constants.GroupIds.CenterSettings);
        }

        public override async Task<IDisplayResult> UpdateAsync(CenterSettings section, BuildEditorContext context)
        {
            if (context.GroupId != Constants.GroupIds.CenterSettings)
            {
                return await EditAsync(section, context);
            }

            var viewModel = new CenterSettingsViewModel();
            await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

            if (viewModel.CenterProfileContentItemId == section.CenterProfileContentItemId)
            {
                return await EditAsync(section, context);
            }

            if (string.IsNullOrEmpty(viewModel.CenterProfileContentItemId))
            {
                if (!string.IsNullOrEmpty(section.CenterProfileContentItemId))
                {
                    await _centerManagerService.DeleteCenterProfileAssignmentAsync(section.CenterProfileContentItemId);
                }

                section.CenterProfileContentItemId = null;

                return await EditAsync(section, context);
            }

            var centerProfileParts = await _centerManagerService.GetCenterProfilesAsync();
            var currentCenterProfilePart = centerProfileParts.FirstOrDefault(x => x.ContentItem.ContentItemId == viewModel.CenterProfileContentItemId);
            if (currentCenterProfilePart == null)
            {
                context.Updater.ModelState.AddModelError(
                    nameof(viewModel.CenterProfileContentItemId),
                    T["A megadott adatlap nem létezik vagy foglalt."]);

                return await EditAsync(section, context);
            }

            try
            {
                var dokiNetMember = await _dokiNetService.GetDokiNetMemberById<DokiNetMember>(currentCenterProfilePart.MemberRightId);
                if (dokiNetMember == null)
                {
                    context.Updater.ModelState.AddModelError(
                        string.Empty,
                        T["A szakellátóhely profilon szereplő szakellátóhely vezető nem található"].Value);

                    return await EditAsync(section, context);
                }

                await _diabetesOrganizationService.ChangeCenterProfileLeaderAsync(dokiNetMember, context.Updater);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMemberById", "MRID:" + currentCenterProfilePart.MemberRightId);
                context.Updater.ModelState.AddModelError(string.Empty, T["Hiba történt a társasági rendszerrel történő kapcsolat során."]);
            }
            catch (DokiNetMemberRegistrationException ex)
            {
                context.Updater.ModelState.AddModelError(string.Empty, ex.Message);
            }

            section.CenterProfileContentItemId = viewModel.CenterProfileContentItemId;
            await _centerManagerService.SetCenterProfileAssignmentAsync(viewModel.CenterProfileContentItemId);

            return await EditAsync(section, context);
        }
    }
}
