using IntelliMed.Core.Exceptions;
using IntelliMed.Core.Extensions;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Services
{
    public class EmailNotificationDeliveryService : IEmailNotificationDeliveryService
    {
        private readonly IEmailNotificationDataService _emailNotificationDataService;
        private readonly IEmailSettingsService _emailSettingsService;
        private readonly IEnumerable<IEmailTemplateProvider> _emailTemplateProviders;
        private readonly IEmailTokenizerDataService _emailTokenizerDataService;
        private readonly ILogger _logger;
        private readonly ISmtpService _smtpService;
        private EmailSettings _emailSettings;


        public EmailNotificationDeliveryService(
            IEmailNotificationDataService emailNotificationDataService,
            IEmailSettingsService emailSettingsService,
            IEnumerable<IEmailTemplateProvider> emailTemplateProviders,
            IEmailTokenizerDataService emailTokenizerDataService,
            ILogger<EmailNotificationDeliveryService> logger,
            ISmtpService smtpService)
        {
            _emailNotificationDataService = emailNotificationDataService;
            _emailSettingsService = emailSettingsService;
            _emailTemplateProviders = emailTemplateProviders;
            _emailTokenizerDataService = emailTokenizerDataService;
            _logger = logger;
            _smtpService = smtpService;
        }


        public async Task SendAsync()
        {
            _emailSettings = await _emailSettingsService.GetEmailSettingsAsync();
            if (!_emailSettings.Enabled)
            {
                _logger.LogInformation("Email delivery service is disabled.", _emailSettings);
            }

            var emailsDequeueLimit = _emailSettings.EmailsDequeueLimit;

            var emails = await _emailNotificationDataService.DequeueAsync(emailsDequeueLimit);
            if (!emails.Any())
            {
                _logger.LogInformation("No emails to send.", nameof(EmailNotificationDeliveryService), nameof(SendAsync));

                return;
            }

            foreach (var email in emails)
            {
                var forceDebugEmail = false;

                var sent = true;
                email.To = new HashSet<string>(email.To.Where(x => x.IsEmail()));
                if (!email.To.Any() && _emailSettings.DebugEmailAddresses.Any())
                {
                    email.To = _emailSettings.DebugEmailAddresses;
                    forceDebugEmail = true;
                }

                if (_emailSettings.Enabled && email.To.Any())
                {
                    sent = await SendNowAsync(email, forceDebugEmail);
                }

                if (sent)
                {
                    await _emailNotificationDataService.MarkAsSentAsync(email.NotificationId);
                }
            }
        }


        private async Task<bool> SendNowAsync(EmailNotification email, bool forceDebugEmail)
        {
            var tokenizedEmail = await _emailTokenizerDataService.GetTokenizedEmailByTemplateIdAsync(email.TemplateId);
            if (tokenizedEmail == null)
            {
                throw new NotFoundException(nameof(email.TemplateId) + ": " + email.TemplateId);
            }

            var i = 0;
            EmailTemplate emailTemplate = null;
            IEmailTemplateProvider emailTemplateProvider = null;
            while (i < _emailTemplateProviders.Count() && emailTemplate == null)
            {
                emailTemplateProvider = _emailTemplateProviders.ElementAt(i++);
                emailTemplate = await emailTemplateProvider.GetEmailTemplateByIdAsync(email.TemplateId);
            }

            if (emailTemplate == null)
            {
                throw new NotFoundException(nameof(emailTemplate));
            }

            var body = await emailTemplateProvider.ProcessAsync(email.TemplateId, email.Data, tokenizedEmail.RawBody);
            body = body.Replace("\n", "<br />");
            if (!string.IsNullOrEmpty(_emailSettings.EmailFooter))
            {
                body += "<br />" + _emailSettings.EmailFooter;
            }

            var message = new MailMessage()
            {
                Body = body,
                IsBodyHtml = true,
                Subject = tokenizedEmail.Subject,
                To = _emailSettings.UseFakeEmails ? "fake-email@nodomain.cx" : string.Join(";", email.To)
            };

            if (forceDebugEmail)
            {
                message.Subject += " !!!kézbesíthetetlen: üres az eredeti címzett";
            }

            var ccs = new HashSet<string>(email.Cc.Concat(_emailSettings.CcEmailAddresses).Where(x => !email.To.Contains(x)));
            if (ccs.Any())
            {
                message.Cc = string.Join(";", ccs);
            }

            var bccs = new HashSet<string>(email.Bcc.Concat(_emailSettings.BccEmailAddresses).Where(x => !email.To.Contains(x) && !ccs.Contains(x)));
            if (bccs.Any())
            {
                message.Bcc = string.Join(";", bccs);
            }

            _logger.LogInformation("Sending email: " + message.To + "|" + message.Subject);
            var result = await _smtpService.SendAsync(message);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email was sent: " + message.To + "|" + message.Subject);

                return true;
            }

            _logger.LogWarning("Email was not sent. " + string.Join(", ", result.Errors.Select(x => x.Value)));

            return false;
        }
    }
}
