using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrganiMedCore.Navigation.Services;
using OrganiMedCore.Organization.BaseTheme.Services;

namespace OrganiMedCore.Organization.BaseTheme
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddScoped<IMenuItemProvider, OrganizationMenuItemProvider>();

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}
