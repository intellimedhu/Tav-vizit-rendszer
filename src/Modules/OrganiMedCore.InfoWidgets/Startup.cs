using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.InfoWidgets.Drivers;
using OrganiMedCore.InfoWidgets.Migrations;
using OrganiMedCore.InfoWidgets.Models;

namespace OrganiMedCore.InfoWidgets
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Parts
            services.AddContentPart<InfoBlockPart>();
            services.AddContentPart<InfoBlockContainerPart>();

            // Drivers
            services.AddContentPartDisplayDriver<InfoBlockPartDisplayDriver>();
            services.AddContentPartDisplayDriver<InfoBlockContainerPartDisplayDriver>();

            // Migrations
            services.AddDataMigration<InfoMigrations>();
        }
    }
}
