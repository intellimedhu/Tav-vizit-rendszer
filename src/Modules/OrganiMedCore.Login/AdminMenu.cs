using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrganiMedCore.Login.Constants;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.Login
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer _stringLocalizer;


        public AdminMenu(IStringLocalizer<AdminMenu> _stringLocalize)
        {
            _stringLocalizer = _stringLocalize;
        }


        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(_stringLocalizer["Bejelentkezés"], "20", settings => settings
                .Id("omcloginsettings")
                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GroupIds.OrganiMedCoreLoginSettings })
                .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                .LocalNav()
            );

            return Task.CompletedTask;
        }
    }
}
