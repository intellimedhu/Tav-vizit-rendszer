using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Settings;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    [BackgroundTask(Schedule = "*/5 * * * *", Description = "Sends a request to doki.Net to check whether any of the membership information that is required for accreditation status calculation procedure has been changed.")]
    public class DokiNetMembershipWatcher : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<DokiNetMembershipWatcher>>();

            try
            {
                var utcNow = serviceProvider.GetRequiredService<IClock>().UtcNow;

                var renewalPeriodService = serviceProvider.GetRequiredService<IRenewalPeriodSettingsService>();
                var periodSettings = await renewalPeriodService.GetCenterRenewalSettingsAsync();

                var currentPeriod = periodSettings.GetCurrentFullPeriod(utcNow);
                if (currentPeriod == null)
                {
                    logger.LogInformation("No renewal period.");

                    // Out of the renewal period, this service must run only once in a week.
                    var weekStart = utcNow.GetWeekStart();
                    if (!utcNow.IsBetweenDateRange(weekStart, weekStart.AddMinutes(7)))
                    {
                        logger.LogInformation("No week start.");

                        return;
                    }
                }

                var siteService = serviceProvider.GetRequiredService<ISiteService>();
                var site = await siteService.GetSiteSettingsAsync();

                var dokiNetService = serviceProvider.GetRequiredService<IDokiNetService>();

                var response = await dokiNetService.WatchMembershipAsync(
                    site.As<DokiNetMembershipWatcherLog>().LastCheckDate ?? currentPeriod.RenewalStartDate);
                if (!response.MemberRightIds.Any())
                {
                    logger.LogInformation("No updated members.");
                }
                else
                {
                    await serviceProvider.GetRequiredService<ICenterProfileService>().OnUserProfilesUpdatedAsync(response.MemberRightIds);
                    logger.LogInformation("Center profiles updated based on MemberRightIds: " + string.Join(',', response.MemberRightIds));
                }

                logger.LogInformation("Updating LastCheckDate: " + response.LastCheckDate.ToString("yyyy-MM-ddTHH:mm:ss.FFFF"));
                site.Alter<DokiNetMembershipWatcherLog>(nameof(DokiNetMembershipWatcherLog), log =>
                {
                    log.LastCheckDate = response.LastCheckDate;
                });

                await siteService.UpdateSiteSettingsAsync(site);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}
