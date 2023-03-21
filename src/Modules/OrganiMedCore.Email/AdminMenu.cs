using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrganiMedCore.Email.Constants;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.Email
{
    public class AdminMenu : INavigationProvider
    {
        public IStringLocalizer T { get; set; }


        public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
        {
            T = stringLocalizer;
        }


        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(T["Email sablonok"], "90", settings => settings
                .Id("omcemailtemplates")
                .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                .Add(T["Sablonok kezelése"], "10", template => template
                    .Action("Index", "Emails", new { area = "OrganiMedCore.Email" })
                    .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                    .LocalNav()
                )
                .Add(T["Beállítások"], "15", template => template
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GroupIds.EmailTemplateSettings })
                    .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                    .LocalNav()
                )
            );

            return Task.CompletedTask;
        }
    }
}
