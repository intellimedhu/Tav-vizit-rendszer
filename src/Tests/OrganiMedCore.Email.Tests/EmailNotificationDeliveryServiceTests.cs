using IntelliMed.Core.Exceptions;
using OrchardCore.Email;
using OrganiMedCore.Email.Models;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Email.Settings;
using OrganiMedCore.Email.ViewModels;
using OrganiMedCore.Testing.Core.MockServices;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.Email.Tests
{
    public class EmailNotificationDeliveryServiceTests
    {
        [Fact]
        public async Task SendAsync_Disabled_ShouldLogDisabled_And_MarkAsSent()
        {
            var guid = Guid.NewGuid();
            var dataService = new NotEmptyEmailNotificationDs2(new[] { new EmailNotification() { NotificationId = guid } });

            var logger = new LoggerMock<EmailNotificationDeliveryService>();
            var service = new EmailNotificationDeliveryService(
                dataService,
                new EmailSettingsServiceMock(1, false),
                null,
                null,
                logger,
                null);

            await service.SendAsync();

            var email = dataService.GetEmailNotificationById(guid);

            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("Email delivery service is disabled."));
            Assert.NotNull(email.SentOn);
        }

        [Fact]
        public async Task SendAsync_NoEmails_ShouldReturnOnly()
        {
            var logger = new LoggerMock<EmailNotificationDeliveryService>();

            var service = new EmailNotificationDeliveryService(
                new EmptyEmailNotificationDs(),
                new EmailSettingsServiceMock(),
                null,
                null,
                logger,
                null);

            await service.SendAsync();

            Assert.Contains(logger.Logs, log => log.State.ToString().Contains("No emails to send."));
        }

        [Fact]
        public async Task SendAsync_TokenizedEmailNotFound_ShouldThrowNotFoundException()
        {
            var service = new EmailNotificationDeliveryService(
                new NotEmptyEmailNotificationDs("MYTEMPLATE_T1"),
                new EmailSettingsServiceMock(),
                null,
                new EmailTokenizerDs(null),
                null,
                null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.SendAsync());
            Assert.Contains("MYTEMPLATE_T1", ex.Message);
        }

        [Fact]
        public async Task SendAsync_EmailTemplateNotFound_ShouldThrowNotFoundException()
        {
            var service = new EmailNotificationDeliveryService(
                new NotEmptyEmailNotificationDs("t1"),
                new EmailSettingsServiceMock(),
                new[] { new EmailTemplateProviderMock(false, null) },
                new EmailTokenizerDs(null),
                null,
                null);

            await Assert.ThrowsAsync<NotFoundException>(() => service.SendAsync());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendAsync_SmtpResult_ShouldBe_AsExpected(bool shouldFail)
        {
            var logger = new LoggerMock<EmailNotificationDeliveryService>();

            var service = new EmailNotificationDeliveryService(
                new NotEmptyEmailNotificationDs("t1"),
                new EmailSettingsServiceMock(),
                new[] { new EmailTemplateProviderMock(true, "This will be the body of the message.") },
                new EmailTokenizerDs(new TokenizedEmail()),
                logger,
                new SmtpServieMock(shouldFail ? SmtpResult.Failed() : SmtpResult.Success));

            await service.SendAsync();

            if (shouldFail)
            {
                Assert.Contains(logger.Logs, log => log.State.ToString().Contains("Email was not sent."));
            }
            else
            {
                Assert.Contains(logger.Logs, log => log.State.ToString().StartsWith("Sending email"));
                Assert.Contains(logger.Logs, log => log.State.ToString().StartsWith("Email was sent"));
            }
        }
    }

    class EmptyEmailNotificationDs : IEmailNotificationDataService
    {
        public Task<IEnumerable<EmailNotification>> DequeueAsync(int limit)
            => Task.FromResult(Enumerable.Empty<EmailNotification>());

        [ExcludeFromCodeCoverage]
        public Task MarkAsSentAsync(Guid notificationId)
            => Task.CompletedTask;

        [ExcludeFromCodeCoverage]
        public Task<Guid> QueueAsync(EmailNotification email)
            => throw new NotImplementedException();
    }

    class NotEmptyEmailNotificationDs : IEmailNotificationDataService
    {
        private readonly string _templateId;


        public NotEmptyEmailNotificationDs(string templateId)
        {
            _templateId = templateId;
        }


        public Task<IEnumerable<EmailNotification>> DequeueAsync(int limit)
        {
            IEnumerable<EmailNotification> emailNotifications = new[]
            {
                new EmailNotification()
                {
                    TemplateId = _templateId
                }
            };

            return Task.FromResult(emailNotifications);
        }

        public Task MarkAsSentAsync(Guid notificationId)
            => Task.CompletedTask;

        [ExcludeFromCodeCoverage]
        public Task<Guid> QueueAsync(EmailNotification email)
            => throw new NotImplementedException();
    }

    class NotEmptyEmailNotificationDs2 : IEmailNotificationDataService
    {
        private readonly IEnumerable<EmailNotification> _emailNotifications;


        public NotEmptyEmailNotificationDs2(IEnumerable<EmailNotification> emailNotifications)
        {
            _emailNotifications = emailNotifications;
        }


        public Task<IEnumerable<EmailNotification>> DequeueAsync(int limit)
            => Task.FromResult(_emailNotifications);

        public Task MarkAsSentAsync(Guid notificationId)
        {
            foreach (var emailNotification in _emailNotifications)
            {
                if (emailNotification.NotificationId == notificationId)
                {
                    emailNotification.SentOn = DateTime.Now;
                }
            }

            return Task.CompletedTask;
        }

        [ExcludeFromCodeCoverage]
        public Task<Guid> QueueAsync(EmailNotification email)
            => throw new NotImplementedException();


        public EmailNotification GetEmailNotificationById(Guid notificationId)
            => _emailNotifications.FirstOrDefault(x => x.NotificationId == notificationId);
    }

    class EmailSettingsServiceMock : IEmailSettingsService
    {
        private readonly int _emailsDequeueLimit;
        private readonly bool _enabled;


        public EmailSettingsServiceMock(int emailsDequeueLimit = 5, bool enabled = true)
        {
            _emailsDequeueLimit = emailsDequeueLimit;
            _enabled = enabled;
        }


        public Task<EmailSettings> GetEmailSettingsAsync()
            => Task.FromResult(new EmailSettings()
            {
                EmailsDequeueLimit = _emailsDequeueLimit,
                Enabled = _enabled,
                EmailFooter = "sample",
                DebugEmailAddresses = new HashSet<string>(new[] { "debug@mail.dx" })
            });
    }

    class EmailTokenizerDs : IEmailTokenizerDataService
    {
        private readonly TokenizedEmail _expectedResult;


        public EmailTokenizerDs(TokenizedEmail expectedResult)
        {
            _expectedResult = expectedResult;
        }


        [ExcludeFromCodeCoverage]
        public Task<EmailTemplate> FindEmailTemplateByIdAsync(string id)
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync()
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task<TokenizedEmail> GetTokenizedEmailByTemplateIdAsync(string id)
            => Task.FromResult(_expectedResult);

        [ExcludeFromCodeCoverage]
        public Task SaveManyTokenizedEmailsAsync(IEnumerable<TokenizedEmailViewModel> viewModels)
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task SaveTokenizedEmailAsync(TokenizedEmailViewModel viewModel)
            => throw new NotImplementedException();
    }

    class EmailTemplateProviderMock : IEmailTemplateProvider
    {
        private readonly bool _shouldFindTemplateById;
        private readonly string _processResult;


        public EmailTemplateProviderMock(bool shouldFindTemplateById, string processResult)
        {
            _shouldFindTemplateById = shouldFindTemplateById;
            _processResult = processResult;
        }


        public Task<EmailTemplate> GetEmailTemplateByIdAsync(string templateId)
        {
            EmailTemplate result = null;
            if (_shouldFindTemplateById)
            {
                result = new EmailTemplate() { Id = templateId };
            }

            return Task.FromResult(result);
        }

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<EmailTemplate>> GetEmailTemplatesAsync()
            => throw new NotImplementedException();

        [ExcludeFromCodeCoverage]
        public Task<string> ProcessAsync(string templateId, object data, string rawBody)
            => Task.FromResult(_processResult);
    }
}
