using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    public interface IEmailTokenizerDataService
    {
        Task<EmailTemplate> FindEmailTemplateByIdAsync(string id);

        Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync();

        Task<TokenizedEmail> GetTokenizedEmailByTemplateIdAsync(string id);

        Task SaveTokenizedEmailAsync(TokenizedEmailViewModel viewModel);

        Task SaveManyTokenizedEmailsAsync(IEnumerable<TokenizedEmailViewModel> viewModels);
    }
}
