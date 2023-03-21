using IntelliMed.Core.Extensions;
using IntelliMed.DokiNetIntegration.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterManager.Drivers;
using OrganiMedCore.DiabetesCareCenterManager.Email;
using OrganiMedCore.DiabetesCareCenterManager.Handlers;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using OrganiMedCore.DiabetesCareCenterManager.Migrations;
using OrganiMedCore.DiabetesCareCenterManager.Services;
using OrganiMedCore.Email.Extensions;

namespace OrganiMedCore.DiabetesCareCenterManager
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Parts
            services.AddContentPart<CenterProfileReviewStatesPart>();
            services.AddContentPart<CenterProfileManagerExtensionsPart>();
            services.AddContentPart<DiabetesUserProfilePart>();

            // Drivers
            services.AddContentPartDisplayDriver<CenterProfileExtensionsPartDisplayDriver>();
            services.AddContentPartDisplayDriver<DiabetesUserProfilePartDisplayDriver>();
            services.AddSiteSettingDisplayDriver<CenterSettingsDisplayDriver>();
            services.AddSiteSettingDisplayDriver<PersonalConditionSettingsDisplayDriver>();

            // Indexes
            services.AddIndexProvider<CenterProfilePartIndexProvider>();
            services.AddIndexProvider<CenterProfileManagerExtensionsPartIndexProvider>();
            services.AddIndexProvider<CenterProfileColleagueIndexProvider>();
            services.AddIndexProvider<TerritoryIndexProvider>();
            services.AddIndexProvider<SettlementIndexProvider>();
            services.AddIndexProvider<DiabetesUserProfilePartIndexProvider>();

            // Migrations
            services.AddDataMigration<CenterProfileMigrations>();
            services.AddDataMigration<CenterProfileManagerExtensionsMigrations>();
            services.AddDataMigration<CenterProfileReviewStatesMirgations>();
            services.AddDataMigration<TerritoryMigrations>();
            services.AddDataMigration<SettlementMigrations>();
            services.AddDataMigration<DiabetesUserProfileMigrations>();

            // Handlers
            services.AddContentPartHandler<CenterProfilePartHandler>();

            // Services
            services.AddScoped<ICenterProfileCommonService, CenterProfileCommonService>();
            services.AddScoped<ICenterProfileService, CenterProfileService>();
            services.AddScoped<ICenterProfileReviewService, CenterProfileReviewService>();
            services.AddScoped<ICenterProfileTenantService, CenterProfileTenantService>();
            services.AddScoped<ICenterUserService, CenterUserService>();
            services.AddScoped<ISettlementService, SettlementService>();
            services.AddScoped<ITerritoryService, TerritoryService>();
            services.AddScoped<IDokiNetMemberValidator, DokiNetMemberValidator>();
            services.AddScoped<IDokiNetUserLoginHandler, DokiNetUserLoginHandler>();
            services.AddScoped<IRenewalPeriodSettingsService, RenewalPeriodSettingsService>();
            services.AddScoped<IQualificationService, QualificationService>();
            services.AddScoped<IRenewalPeriodService, RenewalPeriodService>();
            services.AddScoped<IDiabetesUserProfileService, DiabetesUserProfileService>();
            services.AddScoped<IDccmCrossLoginHandler, DccmCrossLoginHandler>();

            // Permissions
            services.AddPermission<ManagerPermissions>();

            // Background tasks
            services.AddBackgroundTask<DokiNetMembershipWatcher>();
            services.AddBackgroundTask<RenewalPeriodCleanupBackgroundTask>();
            services.AddBackgroundTask<RenewalPeriodNotificationsBackgroundTask>();

            // Navigations
            services.AddNavigation<AdminMenu>();

            // Resources
            services.AddScoped<IResourceManifestProvider, ResourceManifest>();

            // Email related services
            services.AddTemplateProvider<EmailTemplateProvider>();

            services.AddEmailDataProcessor<CenterProfileCreatedEmailProcessor>();
            services.AddEmailDataProcessor<CenterProfileCreatedFeedbackEmailProcessor>();
            services.AddEmailDataProcessor<CenterProfileSubmissionEmailProcessor>();
            services.AddEmailDataProcessor<CenterProfileSubmissionFeedbackEmailProcessor>();
            services.AddEmailDataProcessor<CenterProfileAcceptedEmailProcessor>();
            services.AddEmailDataProcessor<CenterProfileRejectedEmailProcessor>();
            services.AddEmailDataProcessor<CenterProfileLeaderChangedEmailProcessor>();
            services.AddEmailDataProcessor<CenterProfileLeaderChangedBeforeAssignmentEmailProcessor>();
            services.AddEmailDataProcessor<CenterProfileAssignedOnTenantEmailProcessor>();
            services.AddEmailDataProcessor<RenewalPeriodSubmissionReminderEmailProcessor>();

            services.AddEmailDataProcessor<ColleagueActionInvitationAcceptedEmailProcessor>();
            services.AddEmailDataProcessor<ColleagueActionInvitationRejectedEmailProcessor>();
            services.AddEmailDataProcessor<ColleagueActionApplicationSubmittedEmailProcessor>();
            services.AddEmailDataProcessor<ColleagueActionApplicationCancelledEmailProcessor>();

            services.AddEmailDataProcessor<ColleagueActionApplicationByLeaderColleagueInvitationToMemberEmailProcessor>();
            services.AddEmailDataProcessor<ColleagueActionApplicationByLeaderColleagueInvitationToNonMemberEmailProcessor>();

            services.AddEmailDataProcessor<ColleagueActionApplicationByLeaderRemoveActiveEmailProcessor>();
            services.AddEmailDataProcessor<ColleagueActionApplicationByLeaderAcceptApplicationEmailProcessor>();
            services.AddEmailDataProcessor<ColleagueActionApplicationByLeaderRejectApplicationEmailProcessor>();
            services.AddEmailDataProcessor<ColleagueActionApplicationByLeaderCancelInvitationToMemberEmailProcessor>();
            services.AddEmailDataProcessor<ColleagueActionApplicationByLeaderCancelInvitationToNonMemberEmailProcessor>();
        }
    }
}