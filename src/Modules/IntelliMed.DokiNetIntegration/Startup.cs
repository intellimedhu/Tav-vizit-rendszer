using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Drivers;
using IntelliMed.DokiNetIntegration.Indexes;
using IntelliMed.DokiNetIntegration.Migrations;
using IntelliMed.DokiNetIntegration.Services;
using IntelliMed.DokiNetIntegration.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Users.Models;

namespace IntelliMed.DokiNetIntegration
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Drivers
            services.AddSiteSettingDisplayDriver<DokiNetSettingsDisplayDriver>();
            services.AddDisplayDriver<User, DokiNetMemberDisplayDriver>();
            services.AddDisplayDriver<User, DokiNetMemberButtonsDisplayDriver>();

            // Indexes
            services.AddIndexProvider<DokiNetMemberIndexProvider>();

            // Migrations
            services.AddDataMigration<DokiNetMemberMigrations>();

            // Navigations
            services.AddNavigation<AdminMenu>();

            // Services
            services.AddScoped<IDokiNetService, DokiNetService>();
            //services.AddScoped<IDokiNetService, DokiNetServiceMock>();

            // Tag helpers
            services.AddTagHelpers<DokiNetMemberTagHelper>();

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}