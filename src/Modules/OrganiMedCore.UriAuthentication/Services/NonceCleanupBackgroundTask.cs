using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrganiMedCore.UriAuthentication.Services
{
    [BackgroundTask(Schedule = "0 0 * * *", Description = "Service for cleanup deprecated nonces.")]
    public class NonceCleanupBackgroundTask : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var nonceService = serviceProvider.GetRequiredService<INonceService>();

            await nonceService.CleanupAsync();
        }
    }
}
