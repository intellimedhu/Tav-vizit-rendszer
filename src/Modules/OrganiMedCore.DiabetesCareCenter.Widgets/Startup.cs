using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Migrations;
using OrganiMedCore.DiabetesCareCenter.Widgets.Drivers;
using OrganiMedCore.DiabetesCareCenter.Widgets.Filters;
using OrganiMedCore.DiabetesCareCenter.Widgets.Migrations;
using OrganiMedCore.DiabetesCareCenter.Widgets.Models;
using OrganiMedCore.DiabetesCareCenter.Widgets.Services;

namespace OrganiMedCore.DiabetesCareCenter.Widgets
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Parts
            services.AddContentPart<AccreditationStatusBlockPart>();
            services.AddContentPart<CenterProfileEditorInfoBlockPart>();
            services.AddContentPart<CenterProfileEditorInfoBlockContainerPart>();
            services.AddContentPart<CenterProfileEditorInfoPart>();
            services.AddContentPart<CenterProfileOverviewContainerBlockPart>();
            services.AddContentPart<RenewalPeriodCounterPart>();
            services.AddContentPart<UpdatePeriodCounterPart>();

            // Drivers
            services.AddContentPartDisplayDriver<AccreditationStatusBlockPartDisplayDriver>();
            services.AddContentPartDisplayDriver<CenterProfileEditorInfoBlockPartDisplayDriver>();
            services.AddContentPartDisplayDriver<CenterProfileEditorInfoBlockContainerPartDisplayDriver>();
            services.AddContentPartDisplayDriver<CenterProfileOverviewContainerBlockPartDisplayDriver>();

            // Migrations
            services.AddDataMigration<CenterProfileEditorInfoMigrations>();
            services.AddDataMigration<CenterProfileOverviewMigrations>();
            services.AddDataMigration<AccreditationStatusWidgetMigrations>();
            services.AddDataMigration<RenewalPeriodCounterMigrations>();
            services.AddDataMigration<UpdatePeriodCounterMigrations>();

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }

    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
    public class StartupManagerWidgets : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Parts
            services.AddContentPart<CenterProfileStatusBlockPart>();
            services.AddContentPart<CenterProfileListReviewBlockPart>();
            services.AddContentPart<CenterProfileReviewBlockPart>();
            services.AddContentPart<ColleagueWorkplaceBlockPart>();
            services.AddContentPart<ColleagueWorkplaceContainerBlockPart>();

            // Drivers
            services.AddContentPartDisplayDriver<CenterProfileStatusBlockPartDisplayDriver>();
            services.AddContentPartDisplayDriver<CenterProfileListReviewBlockPartDisplayDriver>();
            services.AddContentPartDisplayDriver<CenterProfileReviewBlockPartDisplayDriver>();
            services.AddContentPartDisplayDriver<ColleagueWorkplaceBlockPartDisplayDriver>();
            services.AddContentPartDisplayDriver<ColleagueWorkplaceContainerBlockPartDisplayDriver>();
            services.AddContentPartDisplayDriver<CenterProfileEditorInfoPartManagerDisplayDriver>();
            services.AddContentPartDisplayDriver<RenewalPeriodCounterPartManagerDisplayDriver>();
            services.AddContentPartDisplayDriver<UpdatePeriodCounterPartManagerDisplayDriver>();

            // Migrations
            services.AddDataMigration<CenterProfileReviewBlocksMigrations>();
            services.AddDataMigration<CenterProfileStatusWidgetMigrations>();
            services.AddDataMigration<ColleagueWorkplaceInfoMirgations>();

            // Liquid filters
            services.AddLiquidFilter<CenterProfileReviewFilter>("centerprofile_review_authorized");
            services.AddLiquidFilter<ColleagueWorkplaceZoneFilter>("colleaguestatus_zones");

            // Navigations
            services.AddNavigation<AdminMenu>();

            // Services
            services.AddScoped<ICenterProfileInfoService, CenterProfileInfoManagerService>();
            services.AddSingleton<IColleagueWorkplaceZoneService, ColleagueWorkplaceZoneService>();
        }
    }

    [Feature("OrganiMedCore.Organization.DiabetesCareCenter.Widgets")]
    public class StartupOrganizationWidgets : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Parts
            services.AddContentPart<CenterProfileOverviewInfoPart>();

            // Drivers
            services.AddContentPartDisplayDriver<CenterProfileEditorInfoPartOrganizationDisplayDriver>();
            services.AddContentPartDisplayDriver<CenterProfileOverviewInfoPartDisplayDriver>();
            services.AddContentPartDisplayDriver<RenewalPeriodCounterPartOrganizationDisplayDriver>();
            services.AddContentPartDisplayDriver<UpdatePeriodCounterPartOrganizationDisplayDriver>();

            // Migrations
            services.AddDataMigration<CenterProfileOverviewOrganizationMigrations>();

            // Services
            services.AddScoped<ICenterProfileInfoService, CenterProfileInfoOrganizationService>();
        }
    }
}