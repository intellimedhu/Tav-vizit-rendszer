using Microsoft.AspNetCore.Routing;
using OrchardCore.Security.Permissions;

namespace OrganiMedCore.Navigation.Models
{
    public class MenuItem
    {
        public Permission Permission { get; set; }

        public bool Condition { get; set; }

        public bool IsAspRouted { get; set; }

        public RouteValueDictionary RouteValueDictionary { get; set; }

        public string LinkClasses { get; set; }

        public string Href { get; set; }

        public string FaIcon { get; set; }

        public string Text { get; set; }

        public int Order { get; set; }

        public bool Disabled { get; set; }

        public bool IsActive { get; set; }
    }
}
