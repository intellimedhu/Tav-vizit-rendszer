using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrganiMedCore.Core.Models;
using OrganiMedCore.Core.Services;

namespace OrganiMedCore.Core
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Parts
            services.AddContentPart<EVisitPatientProfilePart>();
            services.AddContentPart<EVisitOrganizationUserProfilePart>();
            
            // Services
            services.AddScoped<IEVisitOrganizationUserProfileService, EVisitOrganizationUserProfileService>();

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}