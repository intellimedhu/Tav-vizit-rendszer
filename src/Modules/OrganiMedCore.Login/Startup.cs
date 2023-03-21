using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrganiMedCore.Login.Constants;
using OrganiMedCore.Login.Drivers;
using OrganiMedCore.Login.LiquidFilters;
using OrganiMedCore.Login.Services;

namespace OrganiMedCore.Login
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Drivers
            services.AddSiteSettingDisplayDriver<OmcLoginSettingsDisplayDriver>();

            // Liquid filters
            services.AddLiquidFilter<WebIdFilter>("web_id");

            // Services
            services.AddScoped<IDokiNetLoginService, DokiNetLoginService>();
            services.AddScoped<IOrganiMedCoreLoginService, OrganiMedCoreLoginService>();
            services.AddScoped<ISharedLoginService, SharedLoginService>();
            services.AddScoped<ISharedUserService, SharedUserService>();

            // Navigations
            services.AddNavigation<AdminMenu>();

            services.ConfigureApplicationCookie(options =>
            {
                // The action will decide which login method will be used
                // as a default login screen based on the OmcLoginSettings.
                options.LoginPath = "/" + NamedRoutes.OrganiMedCoreLoginRouter;
                options.AccessDeniedPath = "/hozzaferes-megtagadva";
            });
        }
    }
}