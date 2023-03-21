using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    // 03:00 every day
    [BackgroundTask(Schedule = "0 3 * * *", Description = "Service for resetting renewal accreditation and center profile statuses.")]
    public class RenewalPeriodCleanupBackgroundTask : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<RenewalPeriodCleanupBackgroundTask>>();
            var utcNow = serviceProvider.GetRequiredService<IClock>().UtcNow;

            try
            {
                await serviceProvider.GetRequiredService<IRenewalPeriodService>()
                    .ResetCenterProfileStatusesAsync(utcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message, utcNow);
            }
        }
    }
}
