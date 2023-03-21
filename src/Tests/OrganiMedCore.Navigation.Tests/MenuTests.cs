using OrganiMedCore.Navigation.Models;
using System;
using System.Linq;
using Xunit;

namespace OrganiMedCore.Navigation.Tests
{
    public class MenuTests
    {
        [Fact]
        public void Add_Should_Throw()
        {
            var menu = new Menu();
            Assert.Throws<ArgumentNullException>(() => menu.Add(null));
        }

        [Fact]
        public void Add_ShouldBeAdded()
        {
            const string href = "x/y";

            var menu = new Menu();
            menu.Add(new MenuItem() { Href = href });
            Assert.Contains(menu.MenuItems, m => m.Href == href);
        }

        [Fact]
        public void Sort_ShouldBeSorted()
        {
            var menu = new Menu();
            menu.Add(new MenuItem() { Order = 3, Href = "0" });
            menu.Add(new MenuItem() { Order = 0, Href = "1" });
            menu.Add(new MenuItem() { Order = 4, Href = "2" });
            menu.Add(new MenuItem() { Order = 2, Href = "3" });
            menu.Add(new MenuItem() { Order = 1, Href = "4" });

            menu.Sort();

            var hrefs = string.Join("", menu.MenuItems.Select(m => m.Href));
            Assert.Equal("14302", hrefs);
        }
    }
}
