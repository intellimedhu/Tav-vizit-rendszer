using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrganiMedCore.UriAuthentication.Drivers;
using OrganiMedCore.UriAuthentication.Services;

namespace OrganiMedCore.UriAuthentication
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Drivers
            services.AddSiteSettingDisplayDriver<NonceSettingsDisplayDriver>();

            // Background tasks
            services.AddBackgroundTask<NonceCleanupBackgroundTask>();

            // Navigations
            services.AddNavigation<AdminMenu>();

            // Services
            services.AddScoped<INonceService, NonceService>();
        }
    }
}
