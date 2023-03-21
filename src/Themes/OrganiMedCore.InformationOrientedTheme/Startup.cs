using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrganiMedCore.Bootstrap.Services;
using OrganiMedCore.InformationOrientedTheme.Services;

namespace OrganiMedCore.InformationOrientedTheme
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<IBoostrapStyleProvider, BoostrapStyleProvider>();

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}
