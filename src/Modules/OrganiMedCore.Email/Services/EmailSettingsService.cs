using OrchardCore.Entities;
using OrchardCore.Settings;
using OrganiMedCore.Email.Settings;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    public class EmailSettingsService : IEmailSettingsService
    {
        private readonly ISiteService _siteService;


        public EmailSettingsService(ISiteService siteService)
        {
            _siteService = siteService;
        }


        public async Task<EmailSettings> GetEmailSettingsAsync()
            => (await _siteService.GetSiteSettingsAsync()).As<EmailSettings>();
    }
}
