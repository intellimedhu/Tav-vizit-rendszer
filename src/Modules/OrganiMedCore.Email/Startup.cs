using IntelliMed.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Recipes;
using OrganiMedCore.Email.Drivers;
using OrganiMedCore.Email.Indexes;
using OrganiMedCore.Email.Migrations;
using OrganiMedCore.Email.Recipes;
using OrganiMedCore.Email.Services;

namespace OrganiMedCore.Email
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Drivers
            services.AddSiteSettingDisplayDriver<EmailSettingsDisplayDriver>();

            // Indexes
            services.AddIndexProvider<EmailNotificationIndexProvider>();

            // Migrations
            services.AddDataMigration<EmailNotificationMigrations>();

            // Services
            services.AddScoped<IEmailDataProcessorFactory, EmailDataProcessorFactory>();
            services.AddScoped<IEmailTokenizerDataService, EmailTokenizerDataService>();
            services.AddScoped<IEmailNotificationDeliveryService, EmailNotificationDeliveryService>();
            services.AddScoped<IEmailSettingsService, EmailSettingsService>();
            services.AddScoped<IEmailNotificationDataService, EmailNotificationDataService>();

            // Background tasks
            services.AddBackgroundTask<EmailNotificationDeliveryBackgroundTask>();

            // Recipes
            services.AddRecipeExecutionStep<TokenizedEmailStep>();

            // Navigations
            services.AddNavigation<AdminMenu>();
        }
    }
}
