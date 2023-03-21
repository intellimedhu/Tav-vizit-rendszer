using IntelliMed.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.Navigation.Models
{
    public class Menu
    {
        public List<MenuItem> MenuItems { get; private set; } = new List<MenuItem>();


        public Menu Add(MenuItem menuItem)
        {
            menuItem.ThrowIfNull();
            MenuItems.Add(menuItem);

            return this;
        }

        public Menu Sort()
        {
            MenuItems = MenuItems.OrderBy(x => x.Order).ToList();

            return this;
        }
    }
}
