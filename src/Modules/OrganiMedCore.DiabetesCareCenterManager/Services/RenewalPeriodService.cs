using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Models;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Constants;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.ViewModels.NotificationTemplates;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.UriAuthentication.Models;
using OrganiMedCore.UriAuthentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class RenewalPeriodService : IRenewalPeriodService
    {
        private readonly ICenterProfileService _centerProfileService;
        private readonly ICenterProfileTenantService _centerProfileTenantService;
        private readonly IDokiNetService _dokiNetService;
        private readonly IEmailNotificationDataService _emailNotificationDataService;
        private readonly IEmailTemplateProvider _emailTemplateProvider;
        private readonly ILogger _logger;
        private readonly INonceService _nonceService;
        private readonly IRenewalPeriodSettingsService _renewalPeriodSettingsService;


        public RenewalPeriodService(
            ICenterProfileService centerProfileService,
            ICenterProfileTenantService centerProfileTenantService,
            IDokiNetService dokiNetService,
            IEmailNotificationDataService emailNotificationDataService,
            IEmailTemplateProvider emailTemplateProvider,
            ILogger<RenewalPeriodService> logger,
            INonceService nonceService,
            IRenewalPeriodSettingsService renewalPeriodSettingsService)
        {
            _centerProfileService = centerProfileService;
            _centerProfileTenantService = centerProfileTenantService;
            _dokiNetService = dokiNetService;
            _emailNotificationDataService = emailNotificationDataService;
            _emailTemplateProvider = emailTemplateProvider;
            _logger = logger;
            _nonceService = nonceService;
            _renewalPeriodSettingsService = renewalPeriodSettingsService;
        }


        public async Task QueueNotificationsAboutUnsubmittedCenterProfile(DateTime utcNow)
        {
            try
            {
                var renewalSettings = await _renewalPeriodSettingsService.GetCenterRenewalSettingsAsync();
                if (renewalSettings == null)
                {
                    _logger.LogInformation("RenewalSettings is null.");

                    return;
                }

                var currentRenewalPeriod = renewalSettings[utcNow];
                if (currentRenewalPeriod == null)
                {
                    _logger.LogInformation("No active renewal period.", renewalSettings.RenewalPeriods, utcNow);

                    return;
                }

                if (currentRenewalPeriod.EmailTimings.All(dateUtc => currentRenewalPeriod.ProcessedTimings.Contains(dateUtc)))
                {
                    _logger.LogInformation("All timing dates have been already processed: " + string.Join(", ", currentRenewalPeriod.EmailTimings));

                    return;
                }

                var currentTiming = currentRenewalPeriod
                    .EmailTimings
                    .OrderBy(dateUtc => dateUtc)
                    .FirstOrDefault(dateUtc =>
                        dateUtc <= utcNow &&
                        !currentRenewalPeriod.ProcessedTimings.Contains(dateUtc));

                if (currentTiming == default)
                {
                    _logger.LogInformation($"No active timing. At: {utcNow} | Timings: " + string.Join("|", currentRenewalPeriod.EmailTimings));

                    return;
                }

                if (await _emailTemplateProvider.GetEmailTemplateByIdAsync(EmailTemplateIds.RenewalPeriodSubmissionReminder) == null)
                {
                    _logger.LogInformation("No template has been set. Skipping current timing.", EmailTemplateIds.RenewalPeriodSubmissionReminder, currentTiming, utcNow);
                }
                else
                {
                    var contentItems = await _centerProfileService.GetUnsubmittedCenterProfilesAsync(currentRenewalPeriod.RenewalStartDate);
                    if (contentItems.Any())
                    {
                        var leaders = await _dokiNetService.GetDokiNetMembersByIds<DokiNetMember>(
                            contentItems.Select(contentItem => contentItem.As<CenterProfilePart>().MemberRightId));

                        foreach (var contentItem in contentItems)
                        {
                            var part = contentItem.As<CenterProfilePart>();
                            var leader = leaders.FirstOrDefault(x => x.MemberRightId == part.MemberRightId);
                            if (leader == null)
                            {
                                _logger.LogWarning("Leader not found. " + part.MemberRightId);

                                continue;
                            }

                            var leaderEmail = leader.Emails.FirstOrDefault();
                            if (!leaderEmail.IsEmail())
                            {
                                _logger.LogWarning("Leader's email is invalid: " + leaderEmail);

                                continue;
                            }

                            var nonce = new Nonce()
                            {
                                RedirectUrl = "~/" + CenterProfileNamedRoutes.CenterProfile_MyCenterProfiles,
                                Type = NonceType.MemberRightId,
                                TypeId = part.MemberRightId
                            };
                            await _nonceService.CreateAsync(nonce);

                            _logger.LogInformation($"Queue email:{EmailTemplateIds.RenewalPeriodSubmissionReminder},{leaderEmail}");
                            await _emailNotificationDataService.QueueAsync(new EmailNotification()
                            {
                                To = new HashSet<string>(new[] { leaderEmail }),
                                TemplateId = EmailTemplateIds.RenewalPeriodSubmissionReminder,
                                Data = new RenewalPeriodSubmissionReminderViewModel()
                                {
                                    LeaderName = leader.FullName,
                                    CenterName = part.CenterName,
                                    Deadline = currentRenewalPeriod.ReviewStartDate,
                                    Nonce = nonce.Value
                                }
                            });
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No unsubmitted cp.");
                    }
                }

                await _renewalPeriodSettingsService.MarkProcessedTimingAsync(currentRenewalPeriod.Id, currentTiming);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "DokiNetService.GetDokiNetMembersByIds");
            }
        }

        public async Task<int> ResetCenterProfileStatusesAsync(DateTime utcNow, bool force = false)
        {
            if (!force)
            {
                var renewalSettings = await _renewalPeriodSettingsService.GetCenterRenewalSettingsAsync();
                if (renewalSettings == null)
                {
                    _logger.LogInformation("RenewalSettings is null.");

                    return 0;
                }

                if (!renewalSettings.RenewalPeriods.Any(x => x.RenewalStartDate.Date == utcNow.Date))
                {
                    _logger.LogInformation($"There is no renewal period that begins today ({utcNow.Date.ToShortDateString()}).");

                    return 0;
                }
            }

            var contentItems = (await _centerProfileService.GetCenterProfilesAsync())
                .Where(contentItem =>
                {
                    var status = contentItem.As<CenterProfileManagerExtensionsPart>().RenewalCenterProfileStatus;

                    return !status.HasValue || status == CenterProfileStatus.MDTAccepted;
                })
                .ToArray();

            if (contentItems.Length > 0)
            {
                foreach (var contentItem in contentItems)
                {
                    await _centerProfileTenantService.RequireCenterProfileContentItemInNewRenewalProcessAsync(contentItem, false);
                }

                if (await _centerProfileService.CacheEnabledAsync())
                {
                    _centerProfileService.ClearCenterProfileCache();
                }
            }

            return contentItems.Length;
        }
    }
}
