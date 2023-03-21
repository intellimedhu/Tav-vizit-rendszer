using OrganiMedCore.Email.Settings;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    public interface IEmailSettingsService
    {
        Task<EmailSettings> GetEmailSettingsAsync();
    }
}
