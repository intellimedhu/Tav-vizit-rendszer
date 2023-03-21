using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrganiMedCore.DiabetesCareCenterTenant.Constants;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterTenant
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

            builder
                .Add(T["Szakellátóhely"], "50", config => config
                    .Id("dccnavlink")
                    .Add(T["Szakellátóhely adatlap"], "1", cr => cr
                        .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = GroupIds.CenterSettings })
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
