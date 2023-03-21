using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrganiMedCore.Core.Drivers;
using OrganiMedCore.Core.Indexes;
using OrganiMedCore.Core.Services;
using OrganiMedCore.Organization.ActionFilters;
using OrganiMedCore.Organization.Drivers;
using OrganiMedCore.Organization.Indexes;
using OrganiMedCore.Organization.Migrations;
using OrganiMedCore.Organization.Models;
using OrganiMedCore.Organization.Services;

namespace OrganiMedCore.Organization
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Parts
            services.AddContentPart<OrganizationUnitPart>();
            services.AddContentPart<MetaDataPart>();
            services.AddContentPart<PatientProfilePart>();
            services.AddContentPart<CheckInPart>();
            services.AddContentPart<OrganizationUserProfilePart>();

            // Drivers
            services.AddSiteSettingDisplayDriver<OrganizationSettingsDisplayDriver>();
            services.AddSiteSettingDisplayDriver<TenantSettingsDisplayDriver>();
            services.AddContentPartDisplayDriver<OrganizationUnitPartDisplay>();
            services.AddContentPartDisplayDriver<MetaDataPartDisplay>();
            services.AddContentPartDisplayDriver<PatientProfilePartDisplay>();
            services.AddContentPartDisplayDriver<OrganizationUserProfilePartDisplayDriver>();

            // Indexes
            services.AddIndexProvider<OrganizationUnitPartIndexProvider>();
            services.AddIndexProvider<MetaDataPartIndexProvider>();
            services.AddIndexProvider<PatientProfilePartIndexProvider>();
            services.AddIndexProvider<CheckInPartIndexProvider>();
            services.AddIndexProvider<AssignedEVisitOrganizationProfileIndexProvider>();
            services.AddIndexProvider<OrganizationUserProfilePartIndexProvider>();

            // Migrations
            services.AddDataMigration<OrganizationUnitMigrations>();
            services.AddDataMigration<MetaDataPartMigrations>();
            services.AddDataMigration<PatientProfileMigrations>();
            services.AddDataMigration<CheckInMigrations>();
            services.AddDataMigration<AssignedEVisitOrganizationProfileMigrations>();
            services.AddDataMigration<OrganizationUserProfileMigrations>();

            // ActionFilters
            services.AddScoped<OrganizationUnitActionFilter>();

            // Services
            services.AddScoped<IRoleNameProvider, OrganizationRoleNameProvider>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IOrganizationUserProfileService, OrganizationUserProfileService>();
            services.AddScoped<IUserTypeService, UserTypeService>();
            services.AddScoped<IOrganizationAuthorizationService, OrganizationAuthorizationService>();
            services.AddScoped<IOrganizationUnitTypeProvider, OrganizationUnitTypeProviderBase>();
            services.AddScoped<IEVisitPatientProfileService, EVisitPatientProfileService>();
            services.AddScoped<ICheckInManager, CheckInManager>();
            services.AddScoped<IAccessLogService, AccessLogService>();

            // Permissions
            services.AddPermission<Permissions>();

            // Navigations
            services.AddNavigation<AdminMenu>();
        }
    }
}