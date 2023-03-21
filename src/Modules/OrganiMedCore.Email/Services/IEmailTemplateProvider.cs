using OrganiMedCore.Email.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    public interface IEmailTemplateProvider
    {
        Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync();

        Task<EmailTemplate> GetEmailTemplateByIdAsync(string templateId);

        Task<string> ProcessAsync(string templateId, object data, string rawBody);
    }
}
