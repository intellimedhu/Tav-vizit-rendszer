using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrganiMedCore.DiabetesCareCenter.Widgets.Constants;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenter.Widgets
{
    [Feature("OrganiMedCore.DiabetesCareCenterManager.Widgets")]
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
                .Add(T["Szakellátóhelyek"], config => config
                    .Add(T["Adatlap szerkesztő infó"], "20", cpSettings => cpSettings
                        .Action("EditSettings", "CenterProfileEditorInfoSettings", new
                        {
                            area = "OrganiMedCore.DiabetesCareCenter.Widgets",
                            contentType = ContentTypes.CenterProfileEditorInfoBlockContainer
                        })
                        .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                        .LocalNav()
                    )
                    .Add(T["Adatlap áttekintő infó"], "21", cpSettings => cpSettings
                        .Action("EditSettings", "CenterProfileEditorInfoSettings", new
                        {
                            area = "OrganiMedCore.DiabetesCareCenter.Widgets",
                            contentType = ContentTypes.CenterProfileOverviewContainerBlock
                        })
                        .Permission(OrchardCore.Admin.Permissions.AccessAdminPanel)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
