using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.Bootstrap.Drivers;
using OrganiMedCore.Bootstrap.Migrations;
using OrganiMedCore.Bootstrap.Models;
using OrganiMedCore.Bootstrap.Services;

namespace OrganiMedCore.Bootstrap
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Parts
            services.AddContentPart<BootstrapStylePart>();

            // Drivers
            services.AddContentPartDisplayDriver<BootstrapStylePartDisplayDriver>();

            // Migrations
            services.AddDataMigration<BootstrapStyleMigrations>();

            // Services
            services.AddSingleton<IBoostrapStyleProvider, DefaultBoostrapStyleProvider>();
        }
    }
}
