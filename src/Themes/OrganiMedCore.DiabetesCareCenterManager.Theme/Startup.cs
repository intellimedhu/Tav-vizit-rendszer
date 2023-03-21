using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;

namespace OrganiMedCore.DiabetesCareCenterManager.Theme
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}