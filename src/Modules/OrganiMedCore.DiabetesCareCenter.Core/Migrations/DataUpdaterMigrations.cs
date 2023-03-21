using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrganiMedCore.Login.Settings;

namespace OrganiMedCore.DiabetesCareCenter.Core.Migrations
{
    public class DataUpdaterMigrations : DataMigration
    {
        private readonly ISiteService _siteService;


        public DataUpdaterMigrations(ISiteService siteService)
        {
            _siteService = siteService;
        }


        public int Create()
        {
            var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
            site.Alter<OmcLoginSettings>(nameof(OmcLoginSettings), settings => settings.DokiNetLoginTitle = "MDT diabet.hu bejelentkezés");
            _siteService.UpdateSiteSettingsAsync(site).GetAwaiter().GetResult();

            return 1;
        }
    }
}
