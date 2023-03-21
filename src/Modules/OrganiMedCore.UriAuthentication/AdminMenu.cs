using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrganiMedCore.UriAuthentication.Constants;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.UriAuthentication
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

            builder
                .Add(T["Nonce"], "70", configuration => configuration
                    .Id("noncesettings")
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GroupIds.NonceSettings })
                    .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                    .LocalNav()
                );

            return Task.CompletedTask;
        }
    }
}
