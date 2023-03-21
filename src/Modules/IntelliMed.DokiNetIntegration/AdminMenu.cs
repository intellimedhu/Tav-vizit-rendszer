using IntelliMed.DokiNetIntegration.Constants;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace IntelliMed.DokiNetIntegration
{
    public class AdminMenu : INavigationProvider
    {
        private readonly ShellSettings _shellSettings;


        public IStringLocalizer T { get; set; }


        public AdminMenu(IStringLocalizer<AdminMenu> _stringLocalizer, ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;

            T = _stringLocalizer;
        }


        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase) ||
                _shellSettings.Name == "Default")
            {
                return Task.CompletedTask;
            }

            builder.Add(T["doki.NET"], "60", config => config
                .Id("dokinetsettings")
                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GroupIds.DokiNetSettings })
                .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                .LocalNav()
            );

            return Task.CompletedTask;
        }
    }
}
