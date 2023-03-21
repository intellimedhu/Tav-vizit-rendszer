using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    [BackgroundTask(Schedule = "5/10 * * * *", Description = "Email notifications delivery service.")]
    public class EmailNotificationDeliveryBackgroundTask : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            try
            {
                var service = serviceProvider.GetRequiredService<IEmailNotificationDeliveryService>();

                await service.SendAsync();
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<EmailNotificationDeliveryBackgroundTask>>();

                logger.LogError(ex, "Hiba történt az email(ek) elküldése során.");
            }
        }
    }
}
