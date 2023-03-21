using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.Manager
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

            builder.Add(T["Manager"], "99", manager => manager
                .AddClass("manager")
                .Id("manager")
                .Add(T["Access log"], "1", accessLog => accessLog
                    .Action("Index", "AccessLog", new { area = "OrganiMedCore.Manager" })
                    .Permission(Permissions.ViewAccessLog)
                    .LocalNav()));

            return Task.CompletedTask;
        }
    }
}
