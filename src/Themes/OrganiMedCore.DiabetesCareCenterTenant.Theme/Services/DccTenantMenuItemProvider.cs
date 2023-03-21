using IntelliMed.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Settings;
using OrganiMedCore.DiabetesCareCenter.Core.Extensions;
using OrganiMedCore.InformationOrientedTheme.Constants;
using OrganiMedCore.Navigation.Models;
using OrganiMedCore.Navigation.Services;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterTenant.Theme.Services
{
    public class DccTenantMenuItemProvider : IMenuItemProvider
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISharedDataAccessorService _sharedDataAccessorService;


        public string MenuId => MenuIds.NavMainMenu;

        public IStringLocalizer T { get; set; }


        public DccTenantMenuItemProvider(
            IAuthorizationService authorizationService,
            ISharedDataAccessorService sharedDataAccessorService,
            IStringLocalizer<DccTenantMenuItemProvider> localizer)
        {
            _authorizationService = authorizationService;
            _sharedDataAccessorService = sharedDataAccessorService;

            T = localizer;
        }


        public async Task BuildMenuAsync(Menu builder, object additionalData = null)
        {
            var dccmBaseUrl = string.Empty;
            using (var scope = await _sharedDataAccessorService.GetCareCenterManagerServiceScopeAsync())
            {
                var siteSettings = await scope.ServiceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync();
                dccmBaseUrl = siteSettings.BaseUrl;
            }

            var mapUrl = dccmBaseUrl;
            if (!mapUrl.EndsWith("/"))
            {
                mapUrl += "/";
            }
            mapUrl += "terkep";

            builder
                .Add(new MenuItem()
                {
                    Href = dccmBaseUrl,
                    LinkClasses = "dcc-nav-link nav-link mr-5",
                    FaIcon = "fas fa-home",
                    Text = T["Vezérlőpult"],
                    Condition = true
                })
                .Add(new MenuItem()
                {
                    Href = mapUrl,
                    LinkClasses = "dcc-nav-link nav-link",
                    FaIcon = "fas fa-map-marker-alt",
                    Text = T["Térképes nézet"],
                    Condition = true
                })
                .Add(new MenuItem()
                {
                    LinkClasses = "dcc-nav-link nav-link ml-5",
                    FaIcon = "fas fa-file-alt",
                    Condition = true,
                    Permission = Permissions.ManageCenterProfile,
                    Text = T["Szakellátóhely adatlap áttekintése"],
                    IsAspRouted = true,
                    RouteValueDictionary = new RouteValueDictionary()
                    {
                        { "area", "OrganiMedCore.DiabetesCareCenterTenant" },
                        { "controller", "CenterProfile" },
                        { "action", "Index" }
                    }
                });
        }
    }
}
