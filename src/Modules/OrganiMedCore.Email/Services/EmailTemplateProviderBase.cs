using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Extensions;
using OrganiMedCore.Email.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    public abstract class EmailTemplateProviderBase : IEmailTemplateProvider
    {
        private readonly IEmailDataProcessorFactory _emailDataProcessorFactory;


        public EmailTemplateProviderBase(IEmailDataProcessorFactory emailDataProcessorFactory)
        {
            _emailDataProcessorFactory = emailDataProcessorFactory;
        }


        public virtual async Task<EmailTemplate> GetEmailTemplateByIdAsync(string templateId)
        {
            templateId.ThrowIfNullOrEmpty();

            return (await GetEmailTemplatesAsync()).FirstOrDefault(x => x.Id == templateId);
        }

        public abstract Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync();

        public virtual async Task<string> ProcessAsync(string templateId, object data, string rawBody)
        {
            templateId.ThrowIfNullOrEmpty();
            data.ThrowIfNull();
            rawBody.ThrowIfNullOrEmpty();

            if (await GetEmailTemplateByIdAsync(templateId) == null)
            {
                throw new NotFoundException($"Template with id not found: {templateId}");
            }

            return await _emailDataProcessorFactory
                .ResolveEmailDataProcessor(templateId)
                .ProcessAsync(data, rawBody);
        }
    }
}
