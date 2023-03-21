using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    [BackgroundTask(Schedule = "0 */1 * * *", Description = "Service for enqueuing email notifications that will be sent to those leaders who haven't been submitted the center profile in the renewal period.")]
    public class RenewalPeriodNotificationsBackgroundTask : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var service = serviceProvider.GetRequiredService<IRenewalPeriodService>();

            await service.QueueNotificationsAboutUnsubmittedCenterProfile(
                serviceProvider.GetRequiredService<IClock>().UtcNow);
        }
    }
}
