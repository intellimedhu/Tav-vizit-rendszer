using System.Collections.Generic;
using System.Linq;

namespace OrganiMedCore.Email.Services
{
    public class EmailDataProcessorFactory : IEmailDataProcessorFactory
    {
        private readonly IEnumerable<IEmailDataProcessor> _emailDataProcessors;


        public EmailDataProcessorFactory(IEnumerable<IEmailDataProcessor> emailDataProcessors)
        {
            _emailDataProcessors = emailDataProcessors;
        }


        public IEmailDataProcessor ResolveEmailDataProcessor(string templateId)
            => _emailDataProcessors.FirstOrDefault(processor => processor.TemplateId == templateId);
    }
}
