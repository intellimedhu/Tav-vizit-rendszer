using OrchardCore.Entities;
using OrchardCore.Settings;
using OrganiMedCore.Login.Settings;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.Login.Extensions
{
    public static class LoginExtensions
    {
        internal static async Task<bool> DokiNetLoginEnabledAsync(this ISiteService siteService)
            => await LoginMethodEnabled(siteService, x => x.UseDokiNetLogin);

        internal static async Task<bool> OrganiMedCoreLoginEnabledAsync(this ISiteService siteService)
            => await LoginMethodEnabled(siteService, x => x.UseOrganiMedCoreLogin);


        private static async Task<bool> LoginMethodEnabled(ISiteService siteService, Func<OmcLoginSettings, bool> property)
        {
            var settings = (await siteService.GetSiteSettingsAsync()).As<OmcLoginSettings>();

            return property(settings);
        }
    }
}
