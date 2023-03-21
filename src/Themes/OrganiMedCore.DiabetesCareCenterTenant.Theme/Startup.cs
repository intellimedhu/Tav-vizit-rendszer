using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrganiMedCore.DiabetesCareCenterTenant.Theme.Services;
using OrganiMedCore.Navigation.Services;

namespace OrganiMedCore.DiabetesCareCenterTenant.Theme
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddScoped<IMenuItemProvider, DccTenantMenuItemProvider>();

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}
