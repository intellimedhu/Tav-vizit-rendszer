using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrganiMedCore.Navigation.Services;
using OrganiMedCore.Organization.DiabetesCare.Services;
using OrganiMedCore.Organization.Services;

namespace OrganiMedCore.Organization.DiabetesCare
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddScoped<IMenuItemProvider, PatientProfileMenuItemProvider>();
            services.AddScoped<IOrganizationUnitTypeProvider, OrganizationUnitTypeProvider>();

            // Permissions
            services.AddPermission<Permissions>();

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}