using Microsoft.Extensions.Localization;
using OrganiMedCore.InformationOrientedTheme.Constants;
using OrganiMedCore.Navigation.Models;
using OrganiMedCore.Navigation.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.Organization.BaseTheme.Services
{
    public class OrganizationMenuItemProvider : IMenuItemProvider
    {
        public string MenuId => MenuIds.NavMainMenu;

        public IStringLocalizer T { get; set; }


        public OrganizationMenuItemProvider(IStringLocalizer<AdminMenu> stringLocalizer)
        {
            T = stringLocalizer;
        }


        public Task BuildMenuAsync(Menu builder, object additionalData = null)
        {
            builder
                .Add(new MenuItem()
                {
                    IsAspRouted = false,
                    Condition = true,
                    Permission = Permissions.ManageReception,
                    Href = "~/recepcio/kereses",
                    Order = 10,
                    Text = T["Recepció"]
                })
                .Add(new MenuItem()
                {
                    IsAspRouted = false,
                    Condition = true,
                    Permission = Permissions.ManagePatinets,
                    Href = "~/paciensek",
                    Order = 20,
                    Text = T["Osztályos lista"]
                });

            return Task.CompletedTask;
        }
    }
}
