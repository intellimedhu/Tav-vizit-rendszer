using IntelliMed.Core.Extensions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.Email.Services
{
    public class EmailTokenizerDataService : IEmailTokenizerDataService
    {
        private readonly IEnumerable<IEmailTemplateProvider> _emailTemplateProviders;
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;


        private const string TokenizedEmailCacheKey = "TokenizedEmailDocument";


        public EmailTokenizerDataService(
            IEnumerable<IEmailTemplateProvider> emailTemplateProviders,
            IMemoryCache memoryCache,
            ISession session,
            ISignal signal)
        {
            _emailTemplateProviders = emailTemplateProviders;
            _memoryCache = memoryCache;
            _session = session;
            _signal = signal;
        }


        public async Task<EmailTemplate> FindEmailTemplateByIdAsync(string id)
        {
            id.ThrowIfNull();

            var i = 0;
            EmailTemplate emailTemplate = null;
            while (i < _emailTemplateProviders.Count() && emailTemplate == null)
            {
                emailTemplate = await _emailTemplateProviders.ElementAt(i).GetEmailTemplateByIdAsync(id);
                i++;
            }

            return emailTemplate;
        }

        public async Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync()
        {
            var results = await Task.WhenAll(_emailTemplateProviders.Select(provider => provider.GetEmailTemplatesAsync()));

            return results.SelectMany(templates => templates);
        }

        public async Task<TokenizedEmail> GetTokenizedEmailByTemplateIdAsync(string id)
        {
            var tokenizedEmailDocument = await LoadTokenizedEmailsFromCacheAsync();

            return tokenizedEmailDocument?.TokenizedEmails.FirstOrDefault(x => x.TemplateId == id);
        }

        public Task SaveTokenizedEmailAsync(TokenizedEmailViewModel viewModel)
            => SaveManyTokenizedEmailsAsync(new[] { viewModel });

        public async Task SaveManyTokenizedEmailsAsync(IEnumerable<TokenizedEmailViewModel> viewModels)
        {
            viewModels.ThrowIfNull();
            if (!viewModels.Any())
            {
                return;
            }

            foreach (var viewModel in viewModels)
            {
                ThrowIfViewModelIsInvalid(viewModel);
            }

            var tokenizedEmailDocument = await LoadTokenizedEmailsFromCacheAsync() ?? new TokenizedEmailDocument();

            foreach (var viewModel in viewModels)
            {
                var tokenizedEmail = tokenizedEmailDocument.TokenizedEmails.FirstOrDefault(x => x.TemplateId == viewModel.TemplateId);
                if (tokenizedEmail == null)
                {
                    tokenizedEmail = new TokenizedEmail();
                    tokenizedEmailDocument.TokenizedEmails.Add(tokenizedEmail);
                }

                viewModel.UpdateModel(tokenizedEmail);
            }

            _session.Save(tokenizedEmailDocument);

            ClearTokenizedEmailsCache();
        }


        private Task<TokenizedEmailDocument> LoadTokenizedEmailsFromCacheAsync()
            => _memoryCache.GetOrCreateAsync("EmailTokenizerDataService:TokenizedEmailDocument", entry =>
            {
                entry.AddExpirationToken(_signal.GetToken(TokenizedEmailCacheKey));

                return _session.Query<TokenizedEmailDocument>().FirstOrDefaultAsync();
            });

        private void ClearTokenizedEmailsCache()
            => _signal.SignalToken(TokenizedEmailCacheKey);

        private void ThrowIfViewModelIsInvalid(TokenizedEmailViewModel viewModel)
        {
            viewModel.ThrowIfNull();
            viewModel.RawBody.ThrowIfNullOrEmpty();
            viewModel.TemplateId.ThrowIfNullOrEmpty();
            viewModel.Subject.ThrowIfNullOrEmpty();
        }
    }
}
