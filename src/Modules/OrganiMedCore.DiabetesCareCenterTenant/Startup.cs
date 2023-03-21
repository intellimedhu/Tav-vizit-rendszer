using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrganiMedCore.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterTenant.Drivers;
using OrganiMedCore.DiabetesCareCenterTenant.Services;

namespace OrganiMedCore.DiabetesCareCenterTenant
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Drivers
            services.AddSiteSettingDisplayDriver<CenterSettingsDisplayDriver>();
            services.AddSiteSettingDisplayDriver<CenterSettingsButtonsDisplayDriver>();

            // Services
            services.AddScoped<ICenterProfileManagerService, CenterProfileManagerService>();            
            services.AddScoped<IRoleNameProvider, CenterRoleNameProvider>();
            services.AddScoped<IDiabetesCareCenterService, DiabetesCareCenterService>();

            // Permissions
            services.AddPermission<Permissions>();

            // Navigations
            services.AddNavigation<AdminMenu>();

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}