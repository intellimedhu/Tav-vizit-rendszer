using OrganiMedCore.Organization.Constants;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization
{
    public class AdminMenu : INavigationProvider
    {
        public IStringLocalizer T { get; set; }


        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }


        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(T["Configuration"], configuration => configuration
                .Add(T["Settings"], settings => settings
                    .Add(T["Tenant"], T["Tenant"], entry => entry
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = OrganiMedCore.Core.Constants.GroupIds.TenantSettings })
                        .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                        .LocalNav()
                    )
                    .Add(T["Organization"], T["Organization"], entry => entry
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GroupIds.OrganizationSettings })
                        .Permission(Permissions.ManageOrganizationSettings)
                        .LocalNav()
                    )
                )
            );

            return Task.CompletedTask;
        }
    }
}
