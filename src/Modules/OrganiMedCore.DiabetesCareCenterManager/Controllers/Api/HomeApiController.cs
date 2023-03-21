using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Extension;
using OrganiMedCore.DiabetesCareCenter.Core.Helpers;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers.Api
{
    [Route("api/home", Name = "HomeApi")]
    public class HomeApiController : Controller
    {
        private readonly ICenterProfileService _centerProfileService;
        private readonly IClock _clock;
        private readonly IDokiNetService _dokiNetService;
        private readonly ILogger _logger;
        private readonly IRenewalPeriodSettingsService _renewalPeriodSettingsService;
        private readonly ITerritoryService _territoryService;


        public IHtmlLocalizer T { get; set; }


        public HomeApiController(
            ICenterProfileService centerProfileService,
            IClock clock,
            IDokiNetService dokiNetService,
            IHtmlLocalizer<HomeApiController> htmlLocalizer,
            ILogger<HomeApiController> logger,
            IRenewalPeriodSettingsService renewalPeriodSettingsService,
            ITerritoryService territoryService)
        {
            _centerProfileService = centerProfileService;
            _clock = clock;
            _dokiNetService = dokiNetService;
            _logger = logger;
            _renewalPeriodSettingsService = renewalPeriodSettingsService;
            _territoryService = territoryService;

            T = htmlLocalizer;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var contentItems = await _centerProfileService.GetCenterProfilesAsync();

            IEnumerable<DokiNetMember> members;
            try
            {
                members = await _dokiNetService.GetDokiNetMembersByIds<DokiNetMember>(
                    contentItems.Select(contentItem => contentItem.As<CenterProfilePart>().MemberRightId).Distinct());
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMembersByIds");

                members = new DokiNetMember[] { };
            }

            var viewModels = contentItems.Select(contentItem =>
            {
                var member = members.FirstOrDefault(m => m.MemberRightId == contentItem.As<CenterProfilePart>().MemberRightId);

                return CenterProfileComplexViewModel.CreateViewModel(
                    contentItem: contentItem,
                    basicData: true,
                    additional: true,
                    renewal: User.Identity.IsAuthenticated,
                    member: member);
            })
            .OrderBy(x => x.BasicData.CenterName);

            var utcNow = _clock.UtcNow;

            var renewalSettings = await _renewalPeriodSettingsService.GetCenterRenewalSettingsAsync();
            var currentFullPeriod = renewalSettings.GetCurrentFullPeriod(utcNow);

            return Ok(new
            {
                CenterTypes = CenterTypeCaptions.GetLocalizedValues(T)
                    .Select(x => new { Type = x.Key, Text = x.Value }),
                AccreditationStatusCodes = AccreditationStatusCaptions.GetLocalizedValues(T)
                    .Select(x => new { Status = x.Key, Text = x.Value }),
                CenterProfileStatusCodes = new
                {
                    Empty = CenterProfileStatusCaptions.GetLocalizedEmptyValues(T).Select(x => new { Status = x.Key, Text = x.Value }),
                    Current = CenterProfileStatusCaptions.GetLocalizedCurrentValues(T).Select(x => new { Status = x.Key, Text = x.Value }),
                    Filled = CenterProfileStatusCaptions.GetLocalizedFilledValues(T).Select(x => new { Status = x.Key, Text = x.Value })
                },
                Authenticated = User.Identity.IsAuthenticated,
                AdditionalData = CenterProfileHelpers.GetAdditionalAdditionalData(T),
                CenterProfiles = viewModels,
                currentFullPeriod?.RenewalStartDate,
                currentFullPeriod?.ReviewEndDate,
                previousRenewalEndDate = renewalSettings.GetPreviousFullPeriod(utcNow)?.ReviewEndDate,
                Counties = (await _territoryService.GetUsedZipCodesByTerritoriesAsync())
                    .ToDictionary(
                        territory => territory.Key.Name,
                        territory => territory.Value
                            .Where(zipCode => viewModels.Any(x => x.BasicData.CenterZipCode == zipCode)))
            });
        }
    }
}
