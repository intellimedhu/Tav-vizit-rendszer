using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrganiMedCore.Navigation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Navigation.Services
{
    public class MenuManager : IMenuManager
    {
        private static string[] Schemes = { "http", "https", "tel", "mailto" };

        private readonly ILogger _logger;
        private readonly IEnumerable<IMenuItemProvider> _menuItemProviders;
        private readonly ShellSettings _shellSettings;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private IUrlHelper _urlHelper;


        public MenuManager(
            ILogger<MenuManager> logger,
            IEnumerable<IMenuItemProvider> menuItemProviders,
            ShellSettings shellSettings,
            IUrlHelperFactory urlHelperFactory)
        {
            _logger = logger;
            _menuItemProviders = menuItemProviders;
            _shellSettings = shellSettings;
            _urlHelperFactory = urlHelperFactory;
        }


        public async Task<IEnumerable<MenuItem>> BuildMenuAsync(
            ActionContext actionContext, 
            string menuId,
            object additionalData = null)
        {
            var builder = new Menu();
            foreach (var menuItemProvider in _menuItemProviders.Where(menuItemProvider => menuItemProvider.MenuId == menuId))
            {
                try
                {
                    await menuItemProvider.BuildMenuAsync(builder, additionalData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An exception occurred while building the menu.", JsonConvert.SerializeObject(menuItemProvider));
                }
            }

            builder.Sort();

            var menuItems = builder.MenuItems;
            foreach (var menuItem in menuItems)
            {
                menuItem.Href = GetUrl(menuItem, menuItem.RouteValueDictionary, actionContext);
            }

            menuItems = menuItems.Where(x => !string.IsNullOrEmpty(x.Href)).ToList();

            return menuItems;
        }


        // Actually this is almost the copy of OrchardCore.Navigation.NavigationManager.GetUrl method.
        private string GetUrl(MenuItem menuItem, RouteValueDictionary routeValueDictionary, ActionContext actionContext)
        {
            string url;
            if (!menuItem.IsAspRouted)
            {
                if (string.IsNullOrEmpty(menuItem.Href))
                {
                    return "#";
                }
                else
                {
                    url = menuItem.Href;
                }
            }
            else
            {
                if (_urlHelper == null)
                {
                    _urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
                }

                url = _urlHelper.RouteUrl(new UrlRouteContext { Values = routeValueDictionary });
            }

            if (!string.IsNullOrEmpty(url) &&
                actionContext?.HttpContext != null &&
                !(url.StartsWith("/") ||
                Schemes.Any(scheme => url.StartsWith(scheme + ":"))))
            {
                if (url.StartsWith("~/"))
                {
                    if (!string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix))
                    {
                        url = _shellSettings.RequestUrlPrefix + "/" + url.Substring(2);
                    }
                    else
                    {
                        url = url.Substring(2);
                    }

                    url = "/" + url;
                }

                //if (!url.StartsWith("#"))
                //{
                //    var appPath = actionContext.HttpContext.Request.PathBase.ToString();
                //    if (appPath == "/")
                //        appPath = "";
                //    url = appPath + "/" + url;
                //}
            }

            return url;
        }
    }
}
