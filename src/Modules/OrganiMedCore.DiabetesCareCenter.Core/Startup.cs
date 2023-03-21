using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrganiMedCore.DiabetesCareCenter.Core.Drivers;
using OrganiMedCore.DiabetesCareCenter.Core.LiquidFilters;
using OrganiMedCore.DiabetesCareCenter.Core.Migrations;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenter.Core.Validators;

namespace OrganiMedCore.DiabetesCareCenterManager.Core
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Parts
            services.AddContentPart<CenterProfilePart>();

            // Drivers            
            services.AddContentPartDisplayDriver<CenterProfilePartDisplayDriver>();
            services.AddContentPartDisplayDriver<CenterProfileManagerExtensionPartDisplayDriver>();

            // Migrations
            services.AddDataMigration<DataUpdaterMigrations>();

            // Liquid filters
            services.AddLiquidFilter<FullRenewalPeriodFilter>("is_full_renewal_period");

            // Services
            services.AddScoped<IAccreditationStatusCalculator, AccreditationStatusCalculator>();
            services.AddSingleton<ICenterProfileValidator, CenterProfileValidator>();
            services.AddSingleton<DiabetesUserProfileValidator>();            

            // Resources
            services.AddScoped<IResourceManifestProvider, DiabetesCareCenter.Core.ResourceManifest>();
        }
    }
}