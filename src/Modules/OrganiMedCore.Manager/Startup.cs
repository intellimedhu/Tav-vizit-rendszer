using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrganiMedCore.Core.Indexes;
using OrganiMedCore.Manager.Drivers;
using OrganiMedCore.Manager.SharedData.Drivers;
using OrganiMedCore.Manager.SharedData.Indexing;
using OrganiMedCore.Manager.SharedData.Migrations;
using OrganiMedCore.Organization.Drivers; // TODO: Ez itt hogyan??

namespace OrganiMedCore.Manager
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Drivers
            services.AddContentPartDisplayDriver<EVisitPatientProfilePartDisplayDriver>();
            services.AddContentPartDisplayDriver<EVisitOrganizationUserProfilePartDisplayDriver>();
            services.AddDisplayDriver<User, EVisitUserDisplayDriver>();
            services.AddDisplayDriver<User, EVisitUserButtonsDisplayDriver>();

            // Indexes
            services.AddIndexProvider<EVisitPatientProfilePartIndexProvider>();
            services.AddIndexProvider<EVisitOrganizationUserProfilePartIndexProvider>();

            // Migrations
            services.AddDataMigration<EVisitPatientProfileMigrations>();

            // Navigations
            services.AddNavigation<AdminMenu>();
            services.AddDataMigration<EVisitOrganizationUserProfileMigrations>();

            // Permissions
            services.AddPermission<Permissions>();

            // Index handler
            services.AddScoped<IContentPartIndexHandler, EVisitPatientProfilePartIndexHandler>();
        }
    }
}